namespace OakERP.Application.AccountsPayable.Invoices.Contracts;

public sealed class ApInvoiceLineInputDto
{
    public string? Description { get; set; }
    public string? AccountNo { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Qty { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public Guid? TaxRateId { get; set; }
    public decimal LineTotal { get; set; }
}
