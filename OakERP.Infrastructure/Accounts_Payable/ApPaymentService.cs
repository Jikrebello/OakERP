using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using OakERP.Application.AccountsPayable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Bank;

namespace OakERP.Infrastructure.Accounts_Payable;

public sealed class ApPaymentService(
    IApPaymentRepository apPaymentRepository,
    IApPaymentAllocationRepository apPaymentAllocationRepository,
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    IBankAccountRepository bankAccountRepository,
    IGlSettingsProvider glSettingsProvider,
    ApPaymentCommandValidator commandValidator,
    ApPaymentSnapshotFactory snapshotFactory,
    IUnitOfWork unitOfWork,
    ILogger<ApPaymentService> logger
) : IApPaymentService
{
    public async Task<ApPaymentCommandResultDTO> CreateAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ApPaymentCreateValidationResult validatedCommand = commandValidator.ValidateCreate(
                command
            );
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                cancellationToken
            );

            var vendor = await vendorRepository.FindNoTrackingAsync(
                command.VendorId,
                cancellationToken
            );
            if (vendor is null)
            {
                return ApPaymentCommandResultDTO.Fail(
                    "Vendor was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!vendor.IsActive)
            {
                return ApPaymentCommandResultDTO.Fail(
                    "AP payments can be created only for active vendors.",
                    HttpStatusCode.BadRequest
                );
            }

            var bankAccount = await bankAccountRepository.FindNoTrackingAsync(
                command.BankAccountId,
                cancellationToken
            );
            if (bankAccount is null)
            {
                return ApPaymentCommandResultDTO.Fail(
                    "Bank account was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!bankAccount.IsActive)
            {
                return ApPaymentCommandResultDTO.Fail(
                    "AP payments can be created only against active bank accounts.",
                    HttpStatusCode.BadRequest
                );
            }

            if (
                !ApSettlementCalculator.MatchesCurrency(
                    bankAccount.CurrencyCode,
                    settings.BaseCurrencyCode
                )
            )
            {
                return ApPaymentCommandResultDTO.Fail(
                    "AP payment capture currently supports only the base currency.",
                    HttpStatusCode.BadRequest
                );
            }

            bool docNoExists = await apPaymentRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            );
            if (docNoExists)
            {
                return ApPaymentCommandResultDTO.Fail(
                    "An AP payment with this document number already exists.",
                    HttpStatusCode.Conflict
                );
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = new ApPayment
                {
                    DocNo = validatedCommand.DocNo,
                    VendorId = command.VendorId,
                    BankAccountId = command.BankAccountId,
                    PaymentDate = command.PaymentDate,
                    Amount = command.Amount,
                    DocStatus = DocStatus.Draft,
                    PostingDate = null,
                    Memo = validatedCommand.Memo,
                    CreatedBy = validatedCommand.PerformedBy,
                    UpdatedBy = validatedCommand.PerformedBy,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };

                await apPaymentRepository.AddAsync(payment);

                var invoiceLoad = await LoadTrackedInvoicesAsync(
                    validatedCommand.Allocations.Select(x => x.ApInvoiceId).ToArray(),
                    payment.VendorId,
                    settings.BaseCurrencyCode,
                    cancellationToken
                );
                if (invoiceLoad.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return invoiceLoad.failure;
                }

                var allocationResult = await ApplyAllocationsAsync(
                    payment,
                    invoiceLoad.invoices!,
                    validatedCommand.Allocations,
                    command.AllocationDate ?? command.PaymentDate,
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
                    payment,
                    invoiceLoad.invoices!.Values,
                    "AP payment created successfully.",
                    allocationResult.settledAmounts,
                    allocationResult.paymentAllocations
                );
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogWarning(
                    "Concurrency failure while creating AP payment {DocNo}. Entries: {Entries}",
                    validatedCommand.DocNo,
                    string.Join(", ", ex.Entries.Select(x => x.Entity.GetType().Name))
                );
                return ApPaymentCommandResultDTO.Fail(
                    "The payment or one of its invoices was modified during allocation.",
                    HttpStatusCode.Conflict
                );
            }
            catch (DbUpdateException ex) when (IsUniqueDocNoViolation(ex))
            {
                await unitOfWork.RollbackAsync();
                return ApPaymentCommandResultDTO.Fail(
                    "An AP payment with this document number already exists.",
                    HttpStatusCode.Conflict
                );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(
                    ex,
                    "Unexpected failure while creating AP payment {DocNo}",
                    validatedCommand.DocNo
                );
                return ApPaymentCommandResultDTO.Fail(
                    "An unexpected error occurred while creating the AP payment.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AP payment.");
            return ApPaymentCommandResultDTO.Fail(
                "An unexpected error occurred while creating the AP payment.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ApPaymentCommandResultDTO> AllocateAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ApPaymentAllocateValidationResult validatedCommand = commandValidator.ValidateAllocate(
                command
            );
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                ApPayment? payment = await apPaymentRepository.GetTrackedForAllocationAsync(
                    command.PaymentId,
                    cancellationToken
                );
                if (payment is null)
                {
                    await unitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDTO.Fail(
                        "AP payment was not found.",
                        HttpStatusCode.NotFound
                    );
                }

                if (payment.DocStatus != DocStatus.Draft)
                {
                    await unitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDTO.Fail(
                        "Only draft AP payments can be allocated in this slice.",
                        HttpStatusCode.BadRequest
                    );
                }

                GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                    cancellationToken
                );
                if (
                    !ApSettlementCalculator.MatchesCurrency(
                        payment.BankAccount.CurrencyCode,
                        settings.BaseCurrencyCode
                    )
                )
                {
                    await unitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDTO.Fail(
                        "AP payment allocation currently supports only payments in the base currency.",
                        HttpStatusCode.BadRequest
                    );
                }

                var invoiceLoad = await LoadTrackedInvoicesAsync(
                    validatedCommand.Allocations.Select(x => x.ApInvoiceId).ToArray(),
                    payment.VendorId,
                    settings.BaseCurrencyCode,
                    cancellationToken
                );
                if (invoiceLoad.failure is not null)
                {
                    await unitOfWork.RollbackAsync();
                    return invoiceLoad.failure;
                }

                var allocationResult = await ApplyAllocationsAsync(
                    payment,
                    invoiceLoad.invoices!,
                    validatedCommand.Allocations,
                    command.AllocationDate ?? payment.PaymentDate,
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
                    payment,
                    invoiceLoad.invoices!.Values,
                    "AP payment allocations saved successfully.",
                    allocationResult.settledAmounts,
                    allocationResult.paymentAllocations
                );
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogWarning(
                    "Concurrency failure while allocating AP payment {PaymentId}. Entries: {Entries}",
                    command.PaymentId,
                    string.Join(", ", ex.Entries.Select(x => x.Entity.GetType().Name))
                );
                return ApPaymentCommandResultDTO.Fail(
                    "The payment or one of its invoices was modified during allocation.",
                    HttpStatusCode.Conflict
                );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(
                    ex,
                    "Unexpected failure while allocating AP payment {PaymentId}",
                    command.PaymentId
                );
                return ApPaymentCommandResultDTO.Fail(
                    "An unexpected error occurred while allocating the AP payment.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected failure before allocating AP payment {PaymentId}",
                command.PaymentId
            );
            return ApPaymentCommandResultDTO.Fail(
                "An unexpected error occurred while allocating the AP payment.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    private async Task<(
        Dictionary<Guid, ApInvoice>? invoices,
        ApPaymentCommandResultDTO? failure
    )> LoadTrackedInvoicesAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        Guid vendorId,
        string baseCurrencyCode,
        CancellationToken cancellationToken
    )
    {
        if (invoiceIds.Count == 0)
        {
            return (new Dictionary<Guid, ApInvoice>(), null);
        }

        IReadOnlyList<ApInvoice> invoices = await apInvoiceRepository.GetTrackedForSettlementAsync(
            invoiceIds,
            cancellationToken
        );

        if (invoices.Count != invoiceIds.Count)
        {
            return (
                null,
                ApPaymentCommandResultDTO.Fail(
                    "One or more AP invoices were not found.",
                    HttpStatusCode.NotFound
                )
            );
        }

        foreach (ApInvoice invoice in invoices)
        {
            if (invoice.DocStatus != DocStatus.Posted)
            {
                return (
                    null,
                    ApPaymentCommandResultDTO.Fail(
                        "Only posted AP invoices can be allocated in this slice.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (invoice.VendorId != vendorId)
            {
                return (
                    null,
                    ApPaymentCommandResultDTO.Fail(
                        "AP payment allocations must reference invoices for the same vendor.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (!ApSettlementCalculator.MatchesCurrency(invoice.CurrencyCode, baseCurrencyCode))
            {
                return (
                    null,
                    ApPaymentCommandResultDTO.Fail(
                        "AP payment allocation currently supports only invoices in the base currency.",
                        HttpStatusCode.BadRequest
                    )
                );
            }

            if (ApSettlementCalculator.GetInvoiceRemainingAmount(invoice) <= 0m)
            {
                return (
                    null,
                    ApPaymentCommandResultDTO.Fail(
                        $"AP invoice {invoice.DocNo} has no remaining balance to allocate.",
                        HttpStatusCode.BadRequest
                    )
                );
            }
        }

        return (invoices.ToDictionary(x => x.Id), null);
    }

    private async Task<(
        ApPaymentCommandResultDTO? failure,
        IReadOnlyDictionary<Guid, decimal> settledAmounts,
        IReadOnlyList<ApPaymentAllocation> paymentAllocations
    )> ApplyAllocationsAsync(
        ApPayment payment,
        IReadOnlyDictionary<Guid, ApInvoice> invoices,
        IReadOnlyList<ApPaymentAllocationInputDTO> allocations,
        DateOnly allocationDate,
        string performedBy
    )
    {
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
        List<ApPaymentAllocation> paymentAllocations = [.. payment.Allocations];
        Dictionary<Guid, decimal> settledAmounts = invoices.ToDictionary(
            x => x.Key,
            x => ApSettlementCalculator.GetInvoiceSettledAmount(x.Value)
        );

        if (allocations.Count == 0)
        {
            return (null, settledAmounts, paymentAllocations);
        }

        decimal requestedTotal = allocations.Sum(x => x.AmountApplied);
        decimal paymentUnappliedAmount = ApSettlementCalculator.GetPaymentUnappliedAmount(payment);

        if (requestedTotal > paymentUnappliedAmount)
        {
            return (
                ApPaymentCommandResultDTO.Fail(
                    "Allocation total exceeds the payment's unapplied amount.",
                    HttpStatusCode.BadRequest
                ),
                settledAmounts,
                paymentAllocations
            );
        }

        payment.UpdatedAt = updatedAt;
        payment.UpdatedBy = performedBy;

        foreach (ApPaymentAllocationInputDTO input in allocations)
        {
            if (!invoices.TryGetValue(input.ApInvoiceId, out ApInvoice? invoice))
            {
                return (
                    ApPaymentCommandResultDTO.Fail(
                        "AP invoice was not found.",
                        HttpStatusCode.NotFound
                    ),
                    settledAmounts,
                    paymentAllocations
                );
            }

            decimal currentSettledAmount = settledAmounts[invoice.Id];
            decimal invoiceRemainingAmount = invoice.DocTotal - currentSettledAmount;
            if (input.AmountApplied > invoiceRemainingAmount)
            {
                return (
                    ApPaymentCommandResultDTO.Fail(
                        $"Allocation amount exceeds the remaining balance for invoice {invoice.DocNo}.",
                        HttpStatusCode.BadRequest
                    ),
                    settledAmounts,
                    paymentAllocations
                );
            }

            var allocation = new ApPaymentAllocation
            {
                ApPaymentId = payment.Id,
                ApInvoiceId = invoice.Id,
                AllocationDate = allocationDate,
                AmountApplied = input.AmountApplied,
            };

            await apPaymentAllocationRepository.AddAsync(allocation);
            paymentAllocations.Add(allocation);

            decimal remainingAfterAllocation = invoiceRemainingAmount - input.AmountApplied;
            settledAmounts[invoice.Id] = currentSettledAmount + input.AmountApplied;
            invoice.UpdatedAt = updatedAt;
            invoice.UpdatedBy = performedBy;

            if (remainingAfterAllocation == 0m)
            {
                invoice.DocStatus = DocStatus.Closed;
            }
        }

        return (null, settledAmounts, paymentAllocations);
    }

    private static bool IsUniqueDocNoViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(
            postgresException.ConstraintName,
            "ix_ap_payments_doc_no",
            StringComparison.Ordinal
        );
}
