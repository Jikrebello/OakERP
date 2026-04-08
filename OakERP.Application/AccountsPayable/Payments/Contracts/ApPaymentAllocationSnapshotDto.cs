namespace OakERP.Application.AccountsPayable.Payments.Contracts;

public sealed class ApPaymentAllocationSnapshotDto
{
    public Guid AllocationId { get; set; }
    public Guid ApInvoiceId { get; set; }
    public DateOnly AllocationDate { get; set; }
    public decimal AmountApplied { get; set; }
}
