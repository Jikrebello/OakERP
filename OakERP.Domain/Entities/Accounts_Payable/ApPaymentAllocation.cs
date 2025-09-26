namespace OakERP.Domain.Entities.Accounts_Payable;

public sealed class ApPaymentAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApPaymentId { get; set; }
    public Guid ApInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }

    public ApPayment Payment { get; set; } = default!;
    public ApInvoice Invoice { get; set; } = default!;
}
