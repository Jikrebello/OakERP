using Microsoft.Extensions.Logging;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;

namespace OakERP.Application.AccountsPayable.Payments.Support;

internal sealed class ApPaymentAllocationWorkflow(
    IApPaymentRepository apPaymentRepository,
    IApPaymentAllocationRepository apPaymentAllocationRepository,
    IApInvoiceRepository apInvoiceRepository,
    ApPaymentServiceDependencies dependencies,
    ILogger<Services.ApPaymentService> logger
)
{
    public async Task<ApPaymentCommandResultDto> ExecuteAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken
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
                    return ApPaymentCommandResultDto.Fail(ApPaymentErrors.PaymentNotFound);
                }

                if (payment.DocStatus != DocStatus.Draft)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ApPaymentCommandResultDto.Fail(ApPaymentErrors.OnlyDraftPaymentsAllowed);
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
                    return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationBaseCurrencyOnly);
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
                        ApPaymentSettlementAdapters.CreateAllocationInputs(validatedCommand.Allocations),
                        command.AllocationDate ?? payment.PaymentDate,
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
                    return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationConcurrencyConflict);
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while allocating AP payment {PaymentId}",
                    command.PaymentId
                );
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.UnexpectedAllocateFailure);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected failure before allocating AP payment {PaymentId}",
                command.PaymentId
            );
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.UnexpectedAllocateFailure);
        }
    }
}
