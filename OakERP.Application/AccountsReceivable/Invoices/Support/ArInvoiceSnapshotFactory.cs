using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Application.AccountsReceivable.Invoices.Support;

public static class ArInvoiceSnapshotFactory
{
    public static ArInvoiceCommandResultDto BuildSuccess(ArInvoice invoice, string message) =>
        ArInvoiceCommandResultDto.SuccessWith(BuildInvoiceSnapshot(invoice), message);

    public static ArInvoiceSnapshotDto BuildInvoiceSnapshot(ArInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return new ArInvoiceSnapshotDto
        {
            InvoiceId = invoice.Id,
            DocNo = invoice.DocNo,
            CustomerId = invoice.CustomerId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            CurrencyCode = invoice.CurrencyCode,
            DocStatus = invoice.DocStatus,
            ShipTo = invoice.ShipTo,
            Memo = invoice.Memo,
            TaxTotal = invoice.TaxTotal,
            DocTotal = invoice.DocTotal,
            Lines =
            [
                .. invoice
                    .Lines.OrderBy(x => x.LineNo)
                    .Select(x => new ArInvoiceLineSnapshotDto
                    {
                        LineId = x.Id,
                        LineNo = x.LineNo,
                        Description = x.Description,
                        RevenueAccount = x.RevenueAccount,
                        ItemId = x.ItemId,
                        TaxRateId = x.TaxRateId,
                        LocationId = x.LocationId,
                        Qty = x.Qty,
                        UnitPrice = x.UnitPrice,
                        LineTotal = x.LineTotal,
                    }),
            ],
        };
    }
}
