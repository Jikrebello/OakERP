namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptAllocationSnapshotDto
{
    public Guid AllocationId { get; set; }
    public Guid ArInvoiceId { get; set; }
    public DateOnly AllocationDate { get; set; }
    public decimal AmountApplied { get; set; }
}
