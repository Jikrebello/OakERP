using OakERP.Domain.Posting.Accounts_Receivable;

namespace OakERP.Domain.Posting;

public interface IPostingEngine
{
    // AR invoice-specific posting logic:
    //   - assumes context has been validated (period open, status, etc.)
    //   - returns what GL + inventory rows SHOULD be written
    PostingEngineResult PostArInvoice(ArInvoicePostingContext context);

    PostingEngineResult PostArReceipt(ArReceiptPostingContext context);

    // Later:
    // PostingEngineResult PostApInvoice(ApInvoicePostingContext context);
    // etc.
}
