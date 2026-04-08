using OakERP.Domain.Posting.AccountsPayable;
using OakERP.Domain.Posting.AccountsReceivable;

namespace OakERP.Domain.Posting;

public interface IPostingEngine
{
    PostingEngineResult PostArInvoice(ArInvoicePostingContext context);

    PostingEngineResult PostArReceipt(ArReceiptPostingContext context);

    PostingEngineResult PostApInvoice(ApInvoicePostingContext context);

    PostingEngineResult PostApPayment(ApPaymentPostingContext context);
}
