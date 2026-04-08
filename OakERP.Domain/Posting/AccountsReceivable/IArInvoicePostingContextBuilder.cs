using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting.AccountsReceivable;

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
