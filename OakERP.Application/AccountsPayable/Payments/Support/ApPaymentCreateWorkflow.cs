using Microsoft.Extensions.Logging;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsPayable.Payments.Support;

internal sealed class ApPaymentCreateWorkflow(
    IApPaymentRepository apPaymentRepository,
    IApPaymentAllocationRepository apPaymentAllocationRepository,
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    IBankAccountRepository bankAccountRepository,
    ApPaymentServiceDependencies dependencies,
    ILogger<Services.ApPaymentService> logger
)
{
    public async Task<ApPaymentCommandResultDto> ExecuteAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken
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
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.VendorNotFound);
            }

            if (!vendor.IsActive)
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.VendorInactive);
            }

            var bankAccount = await bankAccountRepository.FindNoTrackingAsync(
                command.BankAccountId,
                cancellationToken
            );
            if (bankAccount is null)
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.BankAccountNotFound);
            }

            if (!bankAccount.IsActive)
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.BankAccountInactive);
            }

            if (
                !ApSettlementCalculator.MatchesCurrency(
                    bankAccount.CurrencyCode,
                    settings.BaseCurrencyCode
                )
            )
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.BaseCurrencyOnly);
            }

            bool docNoExists = await apPaymentRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            );
            if (docNoExists)
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.DuplicateDocumentNumber);
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
                    UpdatedAt = dependencies.Clock.UtcNow,
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
                        ApPaymentSettlementAdapters.CreateAllocationInputs(validatedCommand.Allocations),
                        command.AllocationDate ?? command.PaymentDate,
                        validatedCommand.PerformedBy,
                        dependencies.Clock.UtcNow,
                        ApPaymentSettlementAdapters.CreateAllocationApplySpec(
                            payment,
                            invoiceLoadInvoices!,
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
                    return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationConcurrencyConflict);
                }

                if (dependencies.PersistenceFailureClassifier.IsApPaymentDocNoConflict(ex))
                {
                    return ApPaymentCommandResultDto.Fail(ApPaymentErrors.DuplicateDocumentNumber);
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while creating AP payment {DocNo}",
                    validatedCommand.DocNo
                );
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.UnexpectedCreateFailure);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AP payment.");
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.UnexpectedCreateFailure);
        }
    }
}
