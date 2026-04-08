namespace OakERP.Application.AccountsReceivable.Receipts.Contracts;

public sealed class ArReceiptAllocationSnapshotDto
{
    public Guid AllocationId { get; set; }
    public Guid ArInvoiceId { get; set; }
    public DateOnly AllocationDate { get; set; }
    public decimal AmountApplied { get; set; }
}
