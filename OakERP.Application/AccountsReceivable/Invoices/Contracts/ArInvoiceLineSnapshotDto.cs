namespace OakERP.Application.AccountsReceivable.Invoices.Contracts;

public sealed class ArInvoiceLineSnapshotDto
{
    public Guid LineId { get; set; }
    public int LineNo { get; set; }
    public string? Description { get; set; }
    public string? RevenueAccount { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? TaxRateId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
