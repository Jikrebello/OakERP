using Microsoft.Extensions.Logging;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsReceivable;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

internal sealed class ArReceiptAllocationWorkflow(
    IArReceiptRepository arReceiptRepository,
    IArReceiptAllocationRepository arReceiptAllocationRepository,
    IArInvoiceRepository arInvoiceRepository,
    ArReceiptServiceDependencies dependencies,
    ILogger<Services.ArReceiptService> logger
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

            await dependencies.UnitOfWork.BeginTransactionAsync();

            try
            {
                ArReceipt? receipt = await arReceiptRepository.GetTrackedForAllocationAsync(
                    command.ReceiptId,
                    cancellationToken
                );
                if (receipt is null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDto.Fail(ArReceiptErrors.ReceiptNotFound);
                }

                if (receipt.DocStatus != DocStatus.Draft)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDto.Fail(ArReceiptErrors.OnlyDraftReceiptsAllowed);
                }

                GlPostingSettings settings = await dependencies.GlSettingsProvider.GetSettingsAsync(
                    cancellationToken
                );
                if (
                    !ArSettlementCalculator.MatchesCurrency(
                        receipt.CurrencyCode,
                        settings.BaseCurrencyCode
                    )
                )
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDto.Fail(ArReceiptErrors.AllocationBaseCurrencyOnly);
                }

                var (invoiceLoadInvoices, invoiceLoadFailure) =
                    await SettlementInvoiceLoader.LoadAsync(
                        [.. validatedCommand.Allocations.Select(x => x.ArInvoiceId)],
                        ArReceiptSettlementAdapters.CreateInvoiceLoadSpec(
                            arInvoiceRepository,
                            receipt.CustomerId,
                            receipt.CurrencyCode
                        ),
                        cancellationToken
                    );
                if (invoiceLoadFailure is not null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return invoiceLoadFailure;
                }

                var (allocationFailure, settledAmounts, receiptAllocations) =
                    await SettlementAllocationApplicator.ApplyAsync(
                        ArReceiptSettlementAdapters.CreateAllocationInputs(validatedCommand.Allocations),
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
                    await dependencies.UnitOfWork.RollbackAsync();
                    return allocationFailure;
                }

                await dependencies.UnitOfWork.SaveChangesAsync(cancellationToken);
                await dependencies.UnitOfWork.CommitAsync();

                return ArReceiptSnapshotFactory.BuildSuccess(
                    receipt,
                    invoiceLoadInvoices!.Values,
                    "AR receipt allocations saved successfully.",
                    settledAmounts,
                    receiptAllocations
                );
            }
            catch (Exception ex)
            {
                await dependencies.UnitOfWork.RollbackAsync();
                if (dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(ex))
                {
                    logger.LogWarning(
                        ex,
                        "Concurrency failure while allocating AR receipt {ReceiptId}",
                        command.ReceiptId
                    );
                    return ArReceiptCommandResultDto.Fail(
                        ArReceiptErrors.AllocationConcurrencyConflict
                    );
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while allocating AR receipt {ReceiptId}",
                    command.ReceiptId
                );
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.UnexpectedAllocateFailure);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected failure before allocating AR receipt {ReceiptId}",
                command.ReceiptId
            );
            return ArReceiptCommandResultDto.Fail(ArReceiptErrors.UnexpectedAllocateFailure);
        }
    }
}
