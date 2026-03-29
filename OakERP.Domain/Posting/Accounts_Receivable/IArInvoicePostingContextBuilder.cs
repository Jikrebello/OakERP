using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Domain.Posting.Accounts_Receivable;

public interface IArInvoicePostingContextBuilder
{
    Task<ArInvoicePostingContext> BuildAsync(
        ArInvoice invoice,
        DateOnly postingDate,
        FiscalPeriod period,
        GlPostingSettings settings,
        PostingRule rule,
        CancellationToken cancellationToken = default
    );
}
