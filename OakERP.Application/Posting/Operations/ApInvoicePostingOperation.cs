using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsPayable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;

namespace OakERP.Application.Posting.Operations;

internal sealed class ApInvoicePostingOperation(
    IApInvoiceRepository apInvoiceRepository,
    IApInvoicePostingContextBuilder contextBuilder,
    PostingOperationSupport support,
    PostingTransactionExecutor transactionExecutor
)
{
    public Task<PostResult> PostAsync(PostCommand command, CancellationToken cancellationToken)
    {
        PostingOperationSupport.EnsureForceDisabled(
            command.Force,
            "Force posting is not supported for AP invoice posting."
        );

        return transactionExecutor.ExecuteAsync(
            async ct =>
            {
                ApInvoice invoice =
                    await apInvoiceRepository.GetTrackedForPostingAsync(command.SourceId, ct)
                    ?? throw new InvalidOperationException("AP invoice was not found.");

                PostingOperationSupport.EnsureDraftStatus(
                    invoice.DocStatus,
                    "Only draft AP invoices can be posted."
                );

                GlPostingSettings settings = await support.GetSettingsAsync(ct);
                DateOnly postingDate = command.PostingDate ?? invoice.InvoiceDate;

                PostingOperationSupport.EnsureBaseCurrency(
                    invoice.CurrencyCode,
                    settings.BaseCurrencyCode,
                    "AP invoice posting currently supports only invoices in the base currency."
                );

                FiscalPeriod period = await support.GetOpenPeriodAsync(postingDate, ct);

                IReadOnlyList<ApInvoiceLine> lines = [.. invoice.Lines.OrderBy(x => x.LineNo)];
                if (lines.Count == 0)
                {
                    throw new InvalidOperationException(
                        "AP invoice requires at least one line to be posted."
                    );
                }

                decimal expectedDocTotal = lines.Sum(x => x.LineTotal) + invoice.TaxTotal;
                if (expectedDocTotal != invoice.DocTotal)
                {
                    throw new InvalidOperationException(
                        "AP invoice totals are inconsistent and cannot be posted."
                    );
                }

                PostingRule rule = await support.GetActiveRuleAsync(DocKind.ApInvoice, ct);

                ApInvoicePostingContext context = await contextBuilder.BuildAsync(
                    invoice,
                    postingDate,
                    period,
                    settings,
                    rule,
                    ct
                );

                PostingEngineResult postingResult = support.PostingEngine.PostApInvoice(context);
                await support.ProcessPostingResultAsync(
                    postingResult,
                    PostingSourceTypes.ApInvoice,
                    inventoryRowsAllowed: false,
                    command.PerformedBy,
                    ct
                );

                invoice.DocStatus = DocStatus.Posted;
                invoice.UpdatedBy = command.PerformedBy;
                invoice.UpdatedAt = DateTimeOffset.UtcNow;

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
            "The AP invoice was modified during posting. It may already be posted.",
            cancellationToken
        );
    }
}
