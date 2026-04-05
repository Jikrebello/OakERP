namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptAllocationSnapshotDTO
{
    public Guid AllocationId { get; set; }
    public Guid ArInvoiceId { get; set; }
    public DateOnly AllocationDate { get; set; }
    public decimal AmountApplied { get; set; }
}
