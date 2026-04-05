using OakERP.Application.AccountsPayable;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Accounts_Payable;

public sealed class ApInvoiceSnapshotFactory
{
    public ApInvoiceCommandResultDTO BuildSuccess(ApInvoice invoice, string message) =>
        ApInvoiceCommandResultDTO.SuccessWith(BuildInvoiceSnapshot(invoice), message);

    public ApInvoiceSnapshotDTO BuildInvoiceSnapshot(ApInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return new ApInvoiceSnapshotDTO
        {
            InvoiceId = invoice.Id,
            DocNo = invoice.DocNo,
            VendorId = invoice.VendorId,
            InvoiceNo = invoice.InvoiceNo,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            CurrencyCode = invoice.CurrencyCode,
            DocStatus = invoice.DocStatus,
            Memo = invoice.Memo,
            TaxTotal = invoice.TaxTotal,
            DocTotal = invoice.DocTotal,
            Lines = invoice
                .Lines.OrderBy(x => x.LineNo)
                .Select(x => new ApInvoiceLineSnapshotDTO
                {
                    LineId = x.Id,
                    LineNo = x.LineNo,
                    Description = x.Description,
                    AccountNo = x.AccountNo ?? string.Empty,
                    Qty = x.Qty,
                    UnitPrice = x.UnitPrice,
                    LineTotal = x.LineTotal,
                })
                .ToList(),
        };
    }
}
