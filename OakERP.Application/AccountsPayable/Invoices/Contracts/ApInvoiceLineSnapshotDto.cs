namespace OakERP.Application.AccountsPayable.Invoices.Contracts;

public sealed class ApInvoiceLineSnapshotDto
{
    public Guid LineId { get; set; }
    public int LineNo { get; set; }
    public string? Description { get; set; }
    public string AccountNo { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
