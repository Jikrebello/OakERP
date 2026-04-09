using Microsoft.Extensions.Logging;
using OakERP.Application.Common.Orchestration;
using OakERP.Application.Settlements.Documents;
using OakERP.Domain.AccountsReceivable;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

internal sealed class ArReceiptAllocationWorkflow(
    IArReceiptRepository arReceiptRepository,
    IArReceiptAllocationRepository arReceiptAllocationRepository,
    IArInvoiceRepository arInvoiceRepository,
    SettlementDocumentWorkflowDependencies dependencies,
    ILogger<ArReceiptService> logger
)
{
    public async Task<ArReceiptCommandResultDto> ExecuteAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ArReceiptAllocateValidationResult validatedCommand =
                ArReceiptCommandValidator.ValidateAllocate(command);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            return await dependencies.AllocateWorkflowRunner.ExecuteAsync(
                async innerCancellationToken =>
                {
                    ArReceipt? receipt = await arReceiptRepository.GetTrackedForAllocationAsync(
                        command.ReceiptId,
                        innerCancellationToken
                    );
                    if (receipt is null)
                    {
                        return ArReceiptCommandResultDto.Fail(ArReceiptErrors.ReceiptNotFound);
                    }

                    ArReceiptCommandResultDto? draftFailure =
                        SettlementDocumentPreconditions.EnsureDraftStatus(
                            receipt.DocStatus,
                            ArReceiptCommandResultDto.Fail(ArReceiptErrors.OnlyDraftReceiptsAllowed)
                        );
                    if (draftFailure is not null)
                    {
                        return draftFailure;
                    }

                    GlPostingSettings settings =
                        await dependencies.GlSettingsProvider.GetSettingsAsync(
                            innerCancellationToken
                        );
                    ArReceiptCommandResultDto? currencyFailure =
                        SettlementDocumentPreconditions.EnsureCurrency(
                            receipt.CurrencyCode,
                            settings.BaseCurrencyCode,
                            ArSettlementCalculator.MatchesCurrency,
                            ArReceiptCommandResultDto.Fail(
                                ArReceiptErrors.AllocationBaseCurrencyOnly
                            )
                        );
                    if (currencyFailure is not null)
                    {
                        return currencyFailure;
                    }

                    var (invoiceLoadInvoices, invoiceLoadFailure) =
                        await SettlementInvoiceLoader.LoadAsync(
                            [.. validatedCommand.Allocations.Select(x => x.ArInvoiceId)],
                            ArReceiptSettlementAdapters.CreateInvoiceLoadSpec(
                                arInvoiceRepository,
                                receipt.CustomerId,
                                receipt.CurrencyCode
                            ),
                            innerCancellationToken
                        );
                    if (invoiceLoadFailure is not null)
                    {
                        return invoiceLoadFailure;
                    }

                    var (allocationFailure, settledAmounts, receiptAllocations) =
                        await SettlementAllocationApplicator.ApplyAsync(
                            ArReceiptSettlementAdapters.CreateAllocationInputs(
                                validatedCommand.Allocations
                            ),
                            command.AllocationDate ?? receipt.ReceiptDate,
                            validatedCommand.PerformedBy,
                            dependencies.Clock.UtcNow,
                            ArReceiptSettlementAdapters.CreateAllocationApplySpec(
                                receipt,
                                invoiceLoadInvoices!,
                                arReceiptAllocationRepository
                            )
                        );
                    if (allocationFailure is not null)
                    {
                        return allocationFailure;
                    }

                    return ArReceiptSnapshotFactory.BuildSuccess(
                        receipt,
                        invoiceLoadInvoices!.Values,
                        "AR receipt allocations saved successfully.",
                        settledAmounts,
                        receiptAllocations
                    );
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            ArReceiptCommandResultDto? translatedFailure = WorkflowFailureTranslator.TryTranslate(
                ex,
                new WorkflowExceptionRule<ArReceiptCommandResultDto>(
                    innerException =>
                        dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(
                            innerException
                        ),
                    innerException =>
                    {
                        logger.LogWarning(
                            innerException,
                            "Concurrency failure while allocating AR receipt {ReceiptId}",
                            command.ReceiptId
                        );
                        return ArReceiptCommandResultDto.Fail(
                            ArReceiptErrors.AllocationConcurrencyConflict
                        );
                    }
                )
            );
            if (translatedFailure is not null)
            {
                return translatedFailure;
            }

            logger.LogError(
                ex,
                "Unexpected failure while allocating AR receipt {ReceiptId}",
                command.ReceiptId
            );
            return ArReceiptCommandResultDto.Fail(ArReceiptErrors.UnexpectedAllocateFailure);
        }
    }
}
