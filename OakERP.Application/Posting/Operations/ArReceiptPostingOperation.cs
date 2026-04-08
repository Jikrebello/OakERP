using OakERP.Common.Enums;
using OakERP.Domain.AccountsReceivable;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

namespace OakERP.Application.Posting.Operations;

internal sealed class ArReceiptPostingOperation(
    IArReceiptRepository arReceiptRepository,
    IArReceiptPostingContextBuilder contextBuilder,
    PostingOperationSupport support,
    PostingTransactionExecutor transactionExecutor
)
{
    public Task<PostResult> PostAsync(PostCommand command, CancellationToken cancellationToken)
    {
        PostingOperationSupport.EnsureForceDisabled(
            command.Force,
            "Force posting is not supported for AR receipt posting."
        );

        return transactionExecutor.ExecuteAsync(
            async ct =>
            {
                ArReceipt receipt =
                    await arReceiptRepository.GetTrackedForPostingAsync(command.SourceId, ct)
                    ?? throw new ResourceNotFoundException("AR receipt was not found.");

                PostingOperationSupport.EnsureDraftStatus(
                    receipt.DocStatus,
                    "Only draft AR receipts can be posted."
                );

                GlPostingSettings settings = await support.GetSettingsAsync(ct);
                DateOnly postingDate = command.PostingDate ?? receipt.ReceiptDate;

                PostingOperationSupport.EnsureBaseCurrency(
                    receipt.CurrencyCode,
                    settings.BaseCurrencyCode,
                    "AR receipt posting currently supports only receipts in the base currency."
                );

                if (receipt.BankAccount is null)
                {
                    throw new PostingInvariantViolationException(
                        "AR receipt bank account was not found."
                    );
                }

                if (!receipt.BankAccount.IsActive)
                {
                    throw new PostingInvariantViolationException(
                        "AR receipt posting requires an active bank account."
                    );
                }

                if (
                    !string.Equals(
                        receipt.BankAccount.CurrencyCode,
                        receipt.CurrencyCode,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    throw new PostingInvariantViolationException(
                        "AR receipt bank account currency must match the receipt currency."
                    );
                }

                decimal allocatedAmount = ArSettlementCalculator.GetReceiptAllocatedAmount(receipt);
                if (allocatedAmount > receipt.Amount)
                {
                    throw new PostingInvariantViolationException(
                        "AR receipt allocations exceed the receipt amount and cannot be posted."
                    );
                }

                FiscalPeriod period = await support.GetOpenPeriodAsync(postingDate, ct);
                PostingRule rule = await support.GetActiveRuleAsync(DocKind.ArReceipt, ct);

                ArReceiptPostingContext context = await contextBuilder.BuildAsync(
                    receipt,
                    postingDate,
                    period,
                    settings,
                    rule,
                    ct
                );

                PostingEngineResult postingResult = support.PostingEngine.PostArReceipt(context);
                await support.ProcessPostingResultAsync(
                    postingResult,
                    PostingSourceTypes.ArReceipt,
                    inventoryRowsAllowed: false,
                    command.PerformedBy,
                    ct
                );

                receipt.DocStatus = DocStatus.Posted;
                receipt.PostingDate = postingDate;
                receipt.UpdatedBy = command.PerformedBy;
                receipt.UpdatedAt = support.Clock.UtcNow;

                return new PostResult(
                    command.DocKind,
                    receipt.Id,
                    receipt.DocNo,
                    postingDate,
                    period.Id,
                    postingResult.GlEntries.Count,
                    postingResult.InventoryMovements.Count
                );
            },
            "The AR receipt was modified during posting. It may already be posted.",
            cancellationToken
        );
    }
}
