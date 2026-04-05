using OakERP.Domain.Posting.Accounts_Payable;
using OakERP.Domain.Posting.Accounts_Receivable;

namespace OakERP.Domain.Posting;

public interface IPostingEngine
{
    PostingEngineResult PostArInvoice(ArInvoicePostingContext context);

    PostingEngineResult PostArReceipt(ArReceiptPostingContext context);

    PostingEngineResult PostApInvoice(ApInvoicePostingContext context);

    PostingEngineResult PostApPayment(ApPaymentPostingContext context);
}
