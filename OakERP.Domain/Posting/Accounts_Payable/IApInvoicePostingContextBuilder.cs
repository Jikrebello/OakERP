using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Domain.Posting.Accounts_Payable;

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
