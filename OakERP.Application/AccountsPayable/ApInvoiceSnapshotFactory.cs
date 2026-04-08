using OakERP.Application.AccountsPayable;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Application.AccountsPayable;

public static class ApInvoiceSnapshotFactory
{
    public static ApInvoiceCommandResultDto BuildSuccess(ApInvoice invoice, string message) =>
        ApInvoiceCommandResultDto.SuccessWith(BuildInvoiceSnapshot(invoice), message);

    public static ApInvoiceSnapshotDto BuildInvoiceSnapshot(ApInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return new ApInvoiceSnapshotDto
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
            Lines =
            [
                .. invoice
                    .Lines.OrderBy(x => x.LineNo)
                    .Select(x => new ApInvoiceLineSnapshotDto
                    {
                        LineId = x.Id,
                        LineNo = x.LineNo,
                        Description = x.Description,
                        AccountNo = x.AccountNo ?? string.Empty,
                        Qty = x.Qty,
                        UnitPrice = x.UnitPrice,
                        LineTotal = x.LineTotal,
                    }),
            ],
        };
    }
}
