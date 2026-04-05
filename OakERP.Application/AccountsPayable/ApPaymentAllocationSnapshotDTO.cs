namespace OakERP.Application.AccountsPayable;

public sealed class ApPaymentAllocationSnapshotDTO
{
    public Guid AllocationId { get; set; }
    public Guid ApInvoiceId { get; set; }
    public DateOnly AllocationDate { get; set; }
    public decimal AmountApplied { get; set; }
}
