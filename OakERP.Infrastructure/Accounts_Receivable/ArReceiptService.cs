using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using OakERP.Application.AccountsReceivable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Bank;

namespace OakERP.Infrastructure.Accounts_Receivable;

public sealed class ArReceiptService(
    IArReceiptRepository arReceiptRepository,
    IArReceiptAllocationRepository arReceiptAllocationRepository,
    IArInvoiceRepository arInvoiceRepository,
    ICustomerRepository customerRepository,
    IBankAccountRepository bankAccountRepository,
    IGlSettingsProvider glSettingsProvider,
    ArReceiptCommandValidator commandValidator,
    ArReceiptSnapshotFactory snapshotFactory,
    IUnitOfWork unitOfWork,
    ILogger<ArReceiptService> logger
) : IArReceiptService
{
    public async Task<ArReceiptCommandResultDTO> CreateAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                cancellationToken
            );
            ArReceiptCreateValidationResult validatedCommand = commandValidator.ValidateCreate(
                command,
                settings
            );
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            var customer = await customerRepository.FindNoTrackingAsync(
                command.CustomerId,
                cancellationToken
            );
            if (customer is null)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Customer was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!customer.IsActive)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "AR receipts can be created only for active customers.",
                    HttpStatusCode.BadRequest
                );
            }

            var bankAccount = await bankAccountRepository.FindNoTrackingAsync(
                command.BankAccountId,
                cancellationToken
            );
            if (bankAccount is null)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Bank account was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!bankAccount.IsActive)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "AR receipts can be created only against active bank accounts.",
                    HttpStatusCode.BadRequest
                );
            }

            if (
                !ArSettlementCalculator.MatchesCurrency(
                    bankAccount.CurrencyCode,
                    validatedCommand.CurrencyCode
                )
            )
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Bank account currency must match the receipt currency.",
                    HttpStatusCode.BadRequest
                );
            }

            bool docNoExists = await arReceiptRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            );
            if (docNoExists)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "An AR receipt with this document number already exists.",
                    HttpStatusCode.Conflict
                );
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var receipt = new ArReceipt
                {
                    DocNo = validatedCommand.DocNo,
                    CustomerId = command.CustomerId,
                    BankAccountId = command.BankAccountId,
                    ReceiptDate = command.ReceiptDate,
                    Amount = command.Amount,
                    CurrencyCode = validatedCommand.CurrencyCode,
                    DocStatus = DocStatus.Draft,
                    Memo = validatedCommand.Memo,
                    CreatedBy = validatedCommand.PerformedBy,
                    UpdatedBy = validatedCommand.PerformedBy,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };

                await arReceiptRepository.AddAsync(receipt);

                var invoiceLoad = await LoadTrackedInvoicesAsync(
                    validatedCommand.Allocations.Select(x => x.ArInvoiceId).ToArray(),
                    receipt.CustomerId,
                    validatedCommand.CurrencyCode,
                    cancellationToken
                );
                if (invoiceLoad.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return invoiceLoad.failure;
                }

                var allocationResult = await ApplyAllocationsAsync(
                    receipt,
                    invoiceLoad.invoices!,
                    validatedCommand.Allocations,
                    command.AllocationDate ?? command.ReceiptDate,
                    validatedCommand.PerformedBy
                );
                if (allocationResult.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return allocationResult.failure;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();

                return snapshotFactory.BuildSuccess(
                    receipt,
                    invoiceLoad.invoices!.Values,
                    "AR receipt created successfully.",
                    allocationResult.settledAmounts,
                    allocationResult.receiptAllocations
                );
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogWarning(
                    "Concurrency failure while creating AR receipt {DocNo}. Entries: {Entries}",
                    validatedCommand.DocNo,
                    string.Join(", ", ex.Entries.Select(x => x.Entity.GetType().Name))
                );
                return ArReceiptCommandResultDTO.Fail(
                    "The receipt or one of its invoices was modified during allocation.",
                    HttpStatusCode.Conflict
                );
            }
            catch (DbUpdateException ex) when (IsUniqueDocNoViolation(ex))
            {
                await unitOfWork.RollbackAsync();
                return ArReceiptCommandResultDTO.Fail(
                    "An AR receipt with this document number already exists.",
                    HttpStatusCode.Conflict
                );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(
                    ex,
                    "Unexpected failure while creating AR receipt {DocNo}",
                    validatedCommand.DocNo
                );
                return ArReceiptCommandResultDTO.Fail(
                    "An unexpected error occurred while creating the AR receipt.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AR receipt.");
            return ArReceiptCommandResultDTO.Fail(
                "An unexpected error occurred while creating the AR receipt.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ArReceiptCommandResultDTO> AllocateAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ArReceiptAllocateValidationResult validatedCommand = commandValidator.ValidateAllocate(
                command
            );
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                ArReceipt? receipt = await arReceiptRepository.GetTrackedForAllocationAsync(
                    command.ReceiptId,
                    cancellationToken
                );
                if (receipt is null)
                {
                    await unitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDTO.Fail(
                        "AR receipt was not found.",
                        HttpStatusCode.NotFound
                    );
                }

                if (receipt.DocStatus != DocStatus.Draft)
                {
                    await unitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDTO.Fail(
                        "Only draft AR receipts can be allocated in this slice.",
                        HttpStatusCode.BadRequest
                    );
                }

                GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                    cancellationToken
                );
                if (
                    !ArSettlementCalculator.MatchesCurrency(
                        receipt.CurrencyCode,
                        settings.BaseCurrencyCode
                    )
                )
                {
                    await unitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDTO.Fail(
                        "AR receipt allocation currently supports only receipts in the base currency.",
                        HttpStatusCode.BadRequest
                    );
                }

                var invoiceLoad = await LoadTrackedInvoicesAsync(
                    validatedCommand.Allocations.Select(x => x.ArInvoiceId).ToArray(),
                    receipt.CustomerId,
                    receipt.CurrencyCode,
                    cancellationToken
                );
                if (invoiceLoad.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return invoiceLoad.failure;
                }

                var allocationResult = await ApplyAllocationsAsync(
                    receipt,
                    invoiceLoad.invoices!,
                    validatedCommand.Allocations,
                    command.AllocationDate ?? receipt.ReceiptDate,
                    validatedCommand.PerformedBy
                );
                if (allocationResult.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return allocationResult.failure;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();

                return snapshotFactory.BuildSuccess(
                    receipt,
                    invoiceLoad.invoices!.Values,
                    "AR receipt allocations saved successfully.",
                    allocationResult.settledAmounts,
                    allocationResult.receiptAllocations
                );
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogWarning(
                    "Concurrency failure while allocating AR receipt {ReceiptId}. Entries: {Entries}",
                    command.ReceiptId,
                    string.Join(", ", ex.Entries.Select(x => x.Entity.GetType().Name))
                );
                return ArReceiptCommandResultDTO.Fail(
                    "The receipt or one of its invoices was modified during allocation.",
                    HttpStatusCode.Conflict
                );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(
                    ex,
                    "Unexpected failure while allocating AR receipt {ReceiptId}",
                    command.ReceiptId
                );
                return ArReceiptCommandResultDTO.Fail(
                    "An unexpected error occurred while allocating the AR receipt.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected failure before allocating AR receipt {ReceiptId}",
                command.ReceiptId
            );
            return ArReceiptCommandResultDTO.Fail(
                "An unexpected error occurred while allocating the AR receipt.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    private async Task<(
        Dictionary<Guid, ArInvoice>? invoices,
        ArReceiptCommandResultDTO? failure
    )> LoadTrackedInvoicesAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        Guid customerId,
        string currencyCode,
        CancellationToken cancellationToken
    )
    {
        if (invoiceIds.Count == 0)
        {
            return (new Dictionary<Guid, ArInvoice>(), null);
        }

        IReadOnlyList<ArInvoice> invoices = await arInvoiceRepository.GetTrackedForAllocationAsync(
            invoiceIds,
            cancellationToken
        );

        if (invoices.Count != invoiceIds.Count)
        {
            return (
                null,
                ArReceiptCommandResultDTO.Fail(
                    "One or more AR invoices were not found.",
                    HttpStatusCode.NotFound
                )
            );
        }

        foreach (ArInvoice invoice in invoices)
        {
            if (invoice.DocStatus != DocStatus.Posted)
            {
                return (
                    null,
                    ArReceiptCommandResultDTO.Fail(
                        "Only posted AR invoices can be allocated in this slice.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (invoice.CustomerId != customerId)
            {
                return (
                    null,
                    ArReceiptCommandResultDTO.Fail(
                        "AR receipt allocations must reference invoices for the same customer.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (!ArSettlementCalculator.MatchesCurrency(invoice.CurrencyCode, currencyCode))
            {
                return (
                    null,
                    ArReceiptCommandResultDTO.Fail(
                        "AR receipt allocations must reference invoices in the same currency as the receipt.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (ArSettlementCalculator.GetInvoiceRemainingAmount(invoice) <= 0m)
            {
                return (
                    null,
                    ArReceiptCommandResultDTO.Fail(
                        $"AR invoice {invoice.DocNo} has no remaining balance to allocate.",
                        HttpStatusCode.BadRequest
                    )
                );
            }
        }

        return (invoices.ToDictionary(x => x.Id), null);
    }

    private async Task<(
        ArReceiptCommandResultDTO? failure,
        IReadOnlyDictionary<Guid, decimal> settledAmounts,
        IReadOnlyList<ArReceiptAllocation> receiptAllocations
    )> ApplyAllocationsAsync(
        ArReceipt receipt,
        IReadOnlyDictionary<Guid, ArInvoice> invoices,
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations,
        DateOnly allocationDate,
        string performedBy
    )
    {
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
        List<ArReceiptAllocation> receiptAllocations = [.. receipt.Allocations];
        Dictionary<Guid, decimal> settledAmounts = invoices.ToDictionary(
            x => x.Key,
            x => ArSettlementCalculator.GetInvoiceSettledAmount(x.Value)
        );

        if (allocations.Count == 0)
        {
            return (null, settledAmounts, receiptAllocations);
        }

        decimal requestedTotal = allocations.Sum(x => x.AmountApplied);
        decimal receiptUnappliedAmount = ArSettlementCalculator.GetReceiptUnappliedAmount(receipt);

        if (requestedTotal > receiptUnappliedAmount)
        {
            return (
                ArReceiptCommandResultDTO.Fail(
                    "Allocation total exceeds the receipt's unapplied amount.",
                    HttpStatusCode.BadRequest
                ),
                settledAmounts,
                receiptAllocations
            );
        }

        receipt.UpdatedAt = updatedAt;
        receipt.UpdatedBy = performedBy;

        foreach (ArReceiptAllocationInputDTO input in allocations)
        {
            if (!invoices.TryGetValue(input.ArInvoiceId, out ArInvoice? invoice))
            {
                return (
                    ArReceiptCommandResultDTO.Fail(
                        "AR invoice was not found.",
                        HttpStatusCode.NotFound
                    ),
                    settledAmounts,
                    receiptAllocations
                );
            }

            decimal currentSettledAmount = settledAmounts[invoice.Id];
            decimal invoiceRemainingAmount = invoice.DocTotal - currentSettledAmount;
            if (input.AmountApplied > invoiceRemainingAmount)
            {
                return (
                    ArReceiptCommandResultDTO.Fail(
                        $"Allocation amount exceeds the remaining balance for invoice {invoice.DocNo}.",
                        HttpStatusCode.BadRequest
                    ),
                    settledAmounts,
                    receiptAllocations
                );
            }

            var allocation = new ArReceiptAllocation
            {
                ArReceiptId = receipt.Id,
                ArInvoiceId = invoice.Id,
                AllocationDate = allocationDate,
                AmountApplied = input.AmountApplied,
            };

            await arReceiptAllocationRepository.AddAsync(allocation);
            receiptAllocations.Add(allocation);

            decimal remainingAfterAllocation = invoiceRemainingAmount - input.AmountApplied;
            settledAmounts[invoice.Id] = currentSettledAmount + input.AmountApplied;
            invoice.UpdatedAt = updatedAt;
            invoice.UpdatedBy = performedBy;

            if (remainingAfterAllocation == 0m)
            {
                invoice.DocStatus = DocStatus.Closed;
            }
        }

        return (null, settledAmounts, receiptAllocations);
    }

    private static bool IsUniqueDocNoViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(
            postgresException.ConstraintName,
            "ix_ar_receipts_doc_no",
            StringComparison.Ordinal
        );
}
