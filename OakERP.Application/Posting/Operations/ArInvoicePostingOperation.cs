using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

namespace OakERP.Application.Posting.Operations;

internal sealed class ArInvoicePostingOperation(
    IArInvoiceRepository arInvoiceRepository,
    IArInvoicePostingContextBuilder contextBuilder,
    PostingOperationSupport support,
    PostingTransactionExecutor transactionExecutor
)
{
    public Task<PostResult> PostAsync(PostCommand command, CancellationToken cancellationToken)
    {
        PostingOperationSupport.EnsureForceDisabled(
            command.Force,
            "Force posting is not supported for AR invoice posting."
        );

        return transactionExecutor.ExecuteAsync(
            async ct =>
            {
                ArInvoice invoice =
                    await arInvoiceRepository.GetTrackedForPostingAsync(command.SourceId, ct)
                    ?? throw new ResourceNotFoundException("AR invoice", command.SourceId.ToString());

                PostingOperationSupport.EnsureDraftStatus(
                    invoice.DocStatus,
                    "Only draft AR invoices can be posted."
                );

                GlPostingSettings settings = await support.GetSettingsAsync(ct);
                DateOnly postingDate = command.PostingDate ?? invoice.InvoiceDate;

                PostingOperationSupport.EnsureBaseCurrency(
                    invoice.CurrencyCode,
                    settings.BaseCurrencyCode,
                    "AR invoice posting currently supports only invoices in the base currency."
                );

                FiscalPeriod period = await support.GetOpenPeriodAsync(postingDate, ct);
                IReadOnlyList<ArInvoiceLine> lines = [.. invoice.Lines.OrderBy(x => x.LineNo)];

                decimal expectedDocTotal = lines.Sum(x => x.LineTotal) + invoice.TaxTotal;
                if (expectedDocTotal != invoice.DocTotal)
                {
                    throw new PostingInvariantViolationException(
                        "AR invoice totals are inconsistent and cannot be posted."
                    );
                }

                PostingRule rule = await support.GetActiveRuleAsync(DocKind.ArInvoice, ct);

                ArInvoicePostingContext context = await contextBuilder.BuildAsync(
                    invoice,
                    postingDate,
                    period,
                    settings,
                    rule,
                    ct
                );

                PostingEngineResult postingResult = support.PostingEngine.PostArInvoice(context);
                await support.ProcessPostingResultAsync(
                    postingResult,
                    PostingSourceTypes.ArInvoice,
                    inventoryRowsAllowed: true,
                    command.PerformedBy,
                    ct
                );

                invoice.DocStatus = DocStatus.Posted;
                invoice.PostingDate = postingDate;
                invoice.UpdatedBy = command.PerformedBy;
                invoice.UpdatedAt = support.Clock.UtcNow;

                return new PostResult(
                    command.DocKind,
                    invoice.Id,
                    invoice.DocNo,
                    postingDate,
                    period.Id,
                    postingResult.GlEntries.Count,
                    postingResult.InventoryMovements.Count
                );
            },
            "The AR invoice was modified during posting. It may already be posted.",
            cancellationToken
        );
    }
}
