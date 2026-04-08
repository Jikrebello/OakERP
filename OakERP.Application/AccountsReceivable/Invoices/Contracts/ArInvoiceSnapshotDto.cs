using OakERP.Common.Enums;

namespace OakERP.Application.AccountsReceivable.Invoices.Contracts;

public sealed class ArInvoiceSnapshotDto
{
    public Guid InvoiceId { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DocStatus DocStatus { get; set; }
    public string? ShipTo { get; set; }
    public string? Memo { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DocTotal { get; set; }
    public IReadOnlyList<ArInvoiceLineSnapshotDto> Lines { get; set; } = [];
}
