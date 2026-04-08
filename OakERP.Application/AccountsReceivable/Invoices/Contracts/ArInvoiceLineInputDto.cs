namespace OakERP.Application.AccountsReceivable.Invoices.Contracts;

public sealed class ArInvoiceLineInputDto
{
    public string? Description { get; set; }
    public string? RevenueAccount { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Qty { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public Guid? TaxRateId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal LineTotal { get; set; }
}
