using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Payable;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Posting.Accounts_Payable;

public sealed class ApInvoicePostingContextBuilder : IApInvoicePostingContextBuilder
{
    public Task<ApInvoicePostingContext> BuildAsync(
        ApInvoice invoice,
        DateOnly postingDate,
        FiscalPeriod period,
        GlPostingSettings settings,
        PostingRule rule,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(rule);

        var lines = new List<ApInvoicePostingLineContext>();

        foreach (ApInvoiceLine line in invoice.Lines.OrderBy(x => x.LineNo))
        {
            if (line.ItemId is not null)
            {
                throw new InvalidOperationException(
                    $"AP invoice line {line.LineNo} uses ItemId and cannot be posted in this slice."
                );
            }

            if (line.TaxRateId is not null)
            {
                throw new InvalidOperationException(
                    $"AP invoice line {line.LineNo} uses TaxRateId and cannot be posted in this slice."
                );
            }

            if (string.IsNullOrWhiteSpace(line.AccountNo))
            {
                throw new InvalidOperationException(
                    $"AP invoice line {line.LineNo} requires an expense account."
                );
            }

            lines.Add(new ApInvoicePostingLineContext(line, line.AccountNo.Trim()));
        }

        return Task.FromResult(
            new ApInvoicePostingContext(invoice, lines, postingDate, period, settings, rule)
        );
    }
}
