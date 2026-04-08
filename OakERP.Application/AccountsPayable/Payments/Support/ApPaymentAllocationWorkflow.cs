using Microsoft.Extensions.Logging;
using OakERP.Application.Common.Orchestration;
using OakERP.Application.Settlements.Documents;
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
    SettlementDocumentWorkflowDependencies dependencies,
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

            return await dependencies.AllocateWorkflowRunner.ExecuteAsync(
                async innerCancellationToken =>
                {
                    ApPayment? payment = await apPaymentRepository.GetTrackedForAllocationAsync(
                        command.PaymentId,
                        innerCancellationToken
                    );
                    if (payment is null)
                    {
                        return ApPaymentCommandResultDto.Fail(ApPaymentErrors.PaymentNotFound);
                    }

                    ApPaymentCommandResultDto? draftFailure =
                        SettlementDocumentPreconditions.EnsureDraftStatus(
                            payment.DocStatus,
                            ApPaymentCommandResultDto.Fail(ApPaymentErrors.OnlyDraftPaymentsAllowed)
                        );
                    if (draftFailure is not null)
                    {
                        return draftFailure;
                    }

                    GlPostingSettings settings =
                        await dependencies.GlSettingsProvider.GetSettingsAsync(
                            innerCancellationToken
                        );
                    ApPaymentCommandResultDto? currencyFailure =
                        SettlementDocumentPreconditions.EnsureCurrency(
                            payment.BankAccount.CurrencyCode,
                            settings.BaseCurrencyCode,
                            ApSettlementCalculator.MatchesCurrency,
                            ApPaymentCommandResultDto.Fail(
                                ApPaymentErrors.AllocationBaseCurrencyOnly
                            )
                        );
                    if (currencyFailure is not null)
                    {
                        return currencyFailure;
                    }

                    var (invoiceLoadInvoices, invoiceLoadFailure) =
                        await SettlementInvoiceLoader.LoadAsync(
                            [.. validatedCommand.Allocations.Select(x => x.ApInvoiceId)],
                            ApPaymentSettlementAdapters.CreateInvoiceLoadSpec(
                                apInvoiceRepository,
                                payment.VendorId,
                                settings.BaseCurrencyCode
                            ),
                            innerCancellationToken
                        );
                    if (invoiceLoadFailure is not null)
                    {
                        return invoiceLoadFailure;
                    }

                    var (allocationFailure, settledAmounts, paymentAllocations) =
                        await SettlementAllocationApplicator.ApplyAsync(
                            ApPaymentSettlementAdapters.CreateAllocationInputs(
                                validatedCommand.Allocations
                            ),
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
                        return allocationFailure;
                    }

                    return ApPaymentSnapshotFactory.BuildSuccess(
                        payment,
                        invoiceLoadInvoices!.Values,
                        "AP payment allocations saved successfully.",
                        settledAmounts,
                        paymentAllocations
                    );
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            ApPaymentCommandResultDto? translatedFailure = WorkflowFailureTranslator.TryTranslate(
                ex,
                [
                    new WorkflowExceptionRule<ApPaymentCommandResultDto>(
                        innerException =>
                            dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(
                                innerException
                            ),
                        innerException =>
                        {
                            logger.LogWarning(
                                innerException,
                                "Concurrency failure while allocating AP payment {PaymentId}",
                                command.PaymentId
                            );
                            return ApPaymentCommandResultDto.Fail(
                                ApPaymentErrors.AllocationConcurrencyConflict
                            );
                        }
                    ),
                ]
            );
            if (translatedFailure is not null)
            {
                return translatedFailure;
            }

            logger.LogError(
                ex,
                "Unexpected failure while allocating AP payment {PaymentId}",
                command.PaymentId
            );
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.UnexpectedAllocateFailure);
        }
    }
}
