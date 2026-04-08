using System.Net;
using Microsoft.Extensions.Logging;
using OakERP.Application.Settlements;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Bank;

namespace OakERP.Application.AccountsPayable;

public sealed class ApPaymentService(
    IApPaymentRepository apPaymentRepository,
    IApPaymentAllocationRepository apPaymentAllocationRepository,
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    IBankAccountRepository bankAccountRepository,
    ApPaymentServiceDependencies dependencies,
    ILogger<ApPaymentService> logger
) : IApPaymentService
{
    public async Task<ApPaymentCommandResultDto> CreateAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ApPaymentCreateValidationResult validatedCommand =
                ApPaymentCommandValidator.ValidateCreate(command);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            GlPostingSettings settings = await dependencies.GlSettingsProvider.GetSettingsAsync(
                cancellationToken
            );

            var vendor = await vendorRepository.FindNoTrackingAsync(
                command.VendorId,
                cancellationToken
            );
            if (vendor is null)
            {
                return ApPaymentCommandResultDto.Fail(
                    "Vendor was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!vendor.IsActive)
            {
                return ApPaymentCommandResultDto.Fail(
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
                return ApPaymentCommandResultDto.Fail(
                    "Bank account was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!bankAccount.IsActive)
            {
                return ApPaymentCommandResultDto.Fail(
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
                return ApPaymentCommandResultDto.Fail(
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
                return ApPaymentCommandResultDto.Fail(
                    "An AP payment with this document number already exists.",
                    HttpStatusCode.Conflict
                );
            }

            await dependencies.UnitOfWork.BeginTransactionAsync();

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

                var (invoiceLoadInvoices, invoiceLoadFailure) =
                    await SettlementInvoiceLoader.LoadAsync(
                        [.. validatedCommand.Allocations.Select(x => x.ApInvoiceId)],
                        ApPaymentSettlementAdapters.CreateInvoiceLoadSpec(
                            apInvoiceRepository,
                            payment.VendorId,
                            settings.BaseCurrencyCode
                        ),
                        cancellationToken
                    );
                if (invoiceLoadFailure is not null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return invoiceLoadFailure;
                }

                var (allocationFailure, settledAmounts, paymentAllocations) =
                    await SettlementAllocationApplicator.ApplyAsync(
                        payment,
                        invoiceLoadInvoices!,
                        validatedCommand.Allocations,
                        command.AllocationDate ?? command.PaymentDate,
                        validatedCommand.PerformedBy,
                        ApPaymentSettlementAdapters.CreateAllocationApplySpec(
                            apPaymentAllocationRepository
                        )
                    );
                if (allocationFailure is not null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return allocationFailure;
                }

                await dependencies.UnitOfWork.SaveChangesAsync(cancellationToken);
                await dependencies.UnitOfWork.CommitAsync();

                return ApPaymentSnapshotFactory.BuildSuccess(
                    payment,
                    invoiceLoadInvoices!.Values,
                    "AP payment created successfully.",
                    settledAmounts,
                    paymentAllocations
                );
            }
            catch (Exception ex)
            {
                await dependencies.UnitOfWork.RollbackAsync();
                if (dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(ex))
                {
                    logger.LogWarning(
                        ex,
                        "Concurrency failure while creating AP payment {DocNo}",
                        validatedCommand.DocNo
                    );
                    return ApPaymentCommandResultDto.Fail(
                        "The payment or one of its invoices was modified during allocation.",
                        HttpStatusCode.Conflict
                    );
                }

                if (
                    dependencies.PersistenceFailureClassifier.IsUniqueConstraint(
                        ex,
                        "ix_ap_payments_doc_no"
                    )
                )
                {
                    return ApPaymentCommandResultDto.Fail(
                        "An AP payment with this document number already exists.",
                        HttpStatusCode.Conflict
                    );
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while creating AP payment {DocNo}",
                    validatedCommand.DocNo
                );
                return ApPaymentCommandResultDto.Fail(
                    "An unexpected error occurred while creating the AP payment.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AP payment.");
            return ApPaymentCommandResultDto.Fail(
                "An unexpected error occurred while creating the AP payment.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ApPaymentCommandResultDto> AllocateAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ApPaymentAllocateValidationResult validatedCommand =
                ApPaymentCommandValidator.ValidateAllocate(command);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            await dependencies.UnitOfWork.BeginTransactionAsync();

            try
            {
                ApPayment? payment = await apPaymentRepository.GetTrackedForAllocationAsync(
                    command.PaymentId,
                    cancellationToken
                );
                if (payment is null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDto.Fail(
                        "AP payment was not found.",
                        HttpStatusCode.NotFound
                    );
                }

                if (payment.DocStatus != DocStatus.Draft)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDto.Fail(
                        "Only draft AP payments can be allocated in this slice.",
                        HttpStatusCode.BadRequest
                    );
                }

                GlPostingSettings settings = await dependencies.GlSettingsProvider.GetSettingsAsync(
                    cancellationToken
                );
                if (
                    !ApSettlementCalculator.MatchesCurrency(
                        payment.BankAccount.CurrencyCode,
                        settings.BaseCurrencyCode
                    )
                )
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDto.Fail(
                        "AP payment allocation currently supports only payments in the base currency.",
                        HttpStatusCode.BadRequest
                    );
                }

                var (invoiceLoadInvoices, invoiceLoadFailure) =
                    await SettlementInvoiceLoader.LoadAsync(
                        [.. validatedCommand.Allocations.Select(x => x.ApInvoiceId)],
                        ApPaymentSettlementAdapters.CreateInvoiceLoadSpec(
                            apInvoiceRepository,
                            payment.VendorId,
                            settings.BaseCurrencyCode
                        ),
                        cancellationToken
                    );
                if (invoiceLoadFailure is not null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return invoiceLoadFailure;
                }

                var (allocationFailure, settledAmounts, paymentAllocations) =
                    await SettlementAllocationApplicator.ApplyAsync(
                        payment,
                        invoiceLoadInvoices!,
                        validatedCommand.Allocations,
                        command.AllocationDate ?? payment.PaymentDate,
                        validatedCommand.PerformedBy,
                        ApPaymentSettlementAdapters.CreateAllocationApplySpec(
                            apPaymentAllocationRepository
                        )
                    );
                if (allocationFailure is not null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return allocationFailure;
                }

                await dependencies.UnitOfWork.SaveChangesAsync(cancellationToken);
                await dependencies.UnitOfWork.CommitAsync();

                return ApPaymentSnapshotFactory.BuildSuccess(
                    payment,
                    invoiceLoadInvoices!.Values,
                    "AP payment allocations saved successfully.",
                    settledAmounts,
                    paymentAllocations
                );
            }
            catch (Exception ex)
            {
                await dependencies.UnitOfWork.RollbackAsync();
                if (dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(ex))
                {
                    logger.LogWarning(
                        ex,
                        "Concurrency failure while allocating AP payment {PaymentId}",
                        command.PaymentId
                    );
                    return ApPaymentCommandResultDto.Fail(
                        "The payment or one of its invoices was modified during allocation.",
                        HttpStatusCode.Conflict
                    );
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while allocating AP payment {PaymentId}",
                    command.PaymentId
                );
                return ApPaymentCommandResultDto.Fail(
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
            return ApPaymentCommandResultDto.Fail(
                "An unexpected error occurred while allocating the AP payment.",
                HttpStatusCode.InternalServerError
            );
        }
    }
}
