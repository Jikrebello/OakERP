using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting.AccountsPayable;

public interface IApInvoicePostingContextBuilder
{
    Task<ApInvoicePostingContext> BuildAsync(
        ApInvoice invoice,
        DateOnly postingDate,
        FiscalPeriod period,
        GlPostingSettings settings,
        PostingRule rule,
        CancellationToken cancellationToken = default
    );
}
