using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using OakERP.Application.AccountsReceivable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
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
            string docNo = command.DocNo.Trim();
            string? memo = NormalizeOptional(command.Memo);
            string currencyCode = NormalizeCurrencyCode(
                command.CurrencyCode,
                settings.BaseCurrencyCode
            );
            string performedBy = GetPerformedBy(command.PerformedBy);
            IReadOnlyList<ArReceiptAllocationInputDTO> allocations = command.Allocations ?? [];

            ArReceiptCommandResultDTO? validationFailure = ValidateCreateRequest(
                command,
                docNo,
                memo,
                currencyCode,
                settings.BaseCurrencyCode,
                allocations
            );
            if (validationFailure is not null)
            {
                return validationFailure;
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

            if (!MatchesCurrency(bankAccount.CurrencyCode, currencyCode))
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Bank account currency must match the receipt currency.",
                    HttpStatusCode.BadRequest
                );
            }

            bool docNoExists = await arReceiptRepository.ExistsDocNoAsync(docNo, cancellationToken);
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
                    DocNo = docNo,
                    CustomerId = command.CustomerId,
                    BankAccountId = command.BankAccountId,
                    ReceiptDate = command.ReceiptDate,
                    Amount = command.Amount,
                    CurrencyCode = currencyCode,
                    DocStatus = DocStatus.Draft,
                    Memo = memo,
                    CreatedBy = performedBy,
                    UpdatedBy = performedBy,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };

                await arReceiptRepository.AddAsync(receipt);

                var invoiceLoad = await LoadTrackedInvoicesAsync(
                    allocations.Select(x => x.ArInvoiceId).ToArray(),
                    receipt.CustomerId,
                    currencyCode,
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
                    allocations,
                    command.AllocationDate ?? command.ReceiptDate,
                    performedBy
                );
                if (allocationResult.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return allocationResult.failure;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();

                return ArReceiptCommandResultDTO.SuccessWith(
                    BuildReceiptSnapshot(receipt, allocationResult.receiptAllocations),
                    BuildInvoiceSnapshots(
                        invoiceLoad.invoices!.Values,
                        allocationResult.settledAmounts
                    ),
                    "AR receipt created successfully."
                );
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogWarning(
                    "Concurrency failure while creating AR receipt {DocNo}. Entries: {Entries}",
                    docNo,
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
                logger.LogError(ex, "Unexpected failure while creating AR receipt {DocNo}", docNo);
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
            IReadOnlyList<ArReceiptAllocationInputDTO> allocations = command.Allocations ?? [];
            ArReceiptCommandResultDTO? validationFailure = ValidateAllocateRequest(
                command,
                allocations
            );
            if (validationFailure is not null)
            {
                return validationFailure;
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
                if (!MatchesCurrency(receipt.CurrencyCode, settings.BaseCurrencyCode))
                {
                    await unitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDTO.Fail(
                        "AR receipt allocation currently supports only receipts in the base currency.",
                        HttpStatusCode.BadRequest
                    );
                }

                var invoiceLoad = await LoadTrackedInvoicesAsync(
                    allocations.Select(x => x.ArInvoiceId).ToArray(),
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
                    allocations,
                    command.AllocationDate ?? receipt.ReceiptDate,
                    GetPerformedBy(command.PerformedBy)
                );
                if (allocationResult.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return allocationResult.failure;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();

                return ArReceiptCommandResultDTO.SuccessWith(
                    BuildReceiptSnapshot(receipt, allocationResult.receiptAllocations),
                    BuildInvoiceSnapshots(
                        invoiceLoad.invoices!.Values,
                        allocationResult.settledAmounts
                    ),
                    "AR receipt allocations saved successfully."
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

            if (!MatchesCurrency(invoice.CurrencyCode, currencyCode))
            {
                return (
                    null,
                    ArReceiptCommandResultDTO.Fail(
                        "AR receipt allocations must reference invoices in the same currency as the receipt.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (GetInvoiceRemainingAmount(invoice) <= 0m)
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
            x => GetInvoiceSettledAmount(x.Value)
        );

        if (allocations.Count == 0)
        {
            return (null, settledAmounts, receiptAllocations);
        }

        decimal requestedTotal = allocations.Sum(x => x.AmountApplied);
        decimal receiptUnappliedAmount = GetReceiptUnappliedAmount(receipt);

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

    private static ArReceiptCommandResultDTO? ValidateCreateRequest(
        CreateArReceiptCommand command,
        string docNo,
        string? memo,
        string currencyCode,
        string baseCurrencyCode,
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations
    )
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return ArReceiptCommandResultDTO.Fail(
                "Document number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (docNo.Length > 40)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Document number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.CustomerId == Guid.Empty)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Customer id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.BankAccountId == Guid.Empty)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Bank account id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.Amount <= 0m)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Receipt amount must be greater than zero.",
                HttpStatusCode.BadRequest
            );
        }

        if (memo is not null && memo.Length > 512)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Receipt memo may not exceed 512 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (!MatchesCurrency(currencyCode, baseCurrencyCode))
        {
            return ArReceiptCommandResultDTO.Fail(
                "AR receipt capture currently supports only the base currency.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: true);
    }

    private static ArReceiptCommandResultDTO? ValidateAllocateRequest(
        AllocateArReceiptCommand command,
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations
    )
    {
        if (command.ReceiptId == Guid.Empty)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Receipt id is required.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: false);
    }

    private static ArReceiptCommandResultDTO? ValidateAllocationInputs(
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations,
        bool allowEmpty
    )
    {
        if (!allowEmpty && allocations.Count == 0)
        {
            return ArReceiptCommandResultDTO.Fail(
                "At least one allocation is required.",
                HttpStatusCode.BadRequest
            );
        }

        HashSet<Guid> invoiceIds = [];

        foreach (ArReceiptAllocationInputDTO allocation in allocations)
        {
            if (allocation.ArInvoiceId == Guid.Empty)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Allocation invoice id is required.",
                    HttpStatusCode.BadRequest
                );
            }

            if (!invoiceIds.Add(allocation.ArInvoiceId))
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Each invoice may appear only once per allocation request.",
                    HttpStatusCode.BadRequest
                );
            }

            if (allocation.AmountApplied <= 0m)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Allocation amount must be greater than zero.",
                    HttpStatusCode.BadRequest
                );
            }
        }

        return null;
    }

    private static ArReceiptSnapshotDTO BuildReceiptSnapshot(
        ArReceipt receipt,
        IReadOnlyCollection<ArReceiptAllocation>? allocationOverrides = null
    )
    {
        IEnumerable<ArReceiptAllocation> allocations = allocationOverrides is not null
            ? allocationOverrides
            : receipt.Allocations;

        return new ArReceiptSnapshotDTO
        {
            ReceiptId = receipt.Id,
            DocNo = receipt.DocNo,
            CustomerId = receipt.CustomerId,
            BankAccountId = receipt.BankAccountId,
            ReceiptDate = receipt.ReceiptDate,
            Amount = receipt.Amount,
            CurrencyCode = receipt.CurrencyCode,
            DocStatus = receipt.DocStatus,
            Memo = receipt.Memo,
            AllocatedAmount = GetReceiptAllocatedAmount(receipt, allocations),
            UnappliedAmount = GetReceiptUnappliedAmount(receipt, allocations),
            Allocations = allocations
                .OrderBy(x => x.AllocationDate)
                .ThenBy(x => x.Id)
                .Select(x => new ArReceiptAllocationSnapshotDTO
                {
                    AllocationId = x.Id,
                    ArInvoiceId = x.ArInvoiceId,
                    AllocationDate = x.AllocationDate,
                    AmountApplied = x.AmountApplied,
                })
                .ToList(),
        };
    }

    private static IReadOnlyList<ArInvoiceSettlementSnapshotDTO> BuildInvoiceSnapshots(
        IEnumerable<ArInvoice> invoices,
        IReadOnlyDictionary<Guid, decimal>? settledAmountOverrides = null
    ) =>
        invoices
            .OrderBy(x => x.DocNo, StringComparer.Ordinal)
            .Select(x =>
            {
                decimal settledAmount =
                    settledAmountOverrides?.TryGetValue(x.Id, out decimal overrideAmount) == true
                        ? overrideAmount
                        : GetInvoiceSettledAmount(x);

                return new ArInvoiceSettlementSnapshotDTO
                {
                    InvoiceId = x.Id,
                    DocNo = x.DocNo,
                    DocStatus = x.DocStatus,
                    DocTotal = x.DocTotal,
                    SettledAmount = settledAmount,
                    RemainingAmount = x.DocTotal - settledAmount,
                };
            })
            .ToList();

    private static decimal GetReceiptAllocatedAmount(
        ArReceipt receipt,
        IEnumerable<ArReceiptAllocation>? allocations = null
    ) => (allocations ?? receipt.Allocations).Sum(x => x.AmountApplied);

    private static decimal GetReceiptUnappliedAmount(
        ArReceipt receipt,
        IEnumerable<ArReceiptAllocation>? allocations = null
    ) => receipt.Amount - GetReceiptAllocatedAmount(receipt, allocations);

    private static decimal GetInvoiceSettledAmount(ArInvoice invoice) =>
        invoice.Allocations.Sum(x =>
            x.AmountApplied + (x.DiscountGiven ?? 0m) + (x.WriteOffAmount ?? 0m)
        );

    private static decimal GetInvoiceRemainingAmount(ArInvoice invoice) =>
        invoice.DocTotal - GetInvoiceSettledAmount(invoice);

    private static string NormalizeCurrencyCode(string? currencyCode, string baseCurrencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode)
            ? baseCurrencyCode.ToUpperInvariant()
            : currencyCode.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetPerformedBy(string? performedBy) =>
        string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim();

    private static bool MatchesCurrency(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool IsUniqueDocNoViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(
            postgresException.ConstraintName,
            "ix_ar_receipts_doc_no",
            StringComparison.Ordinal
        );
}
