namespace OakERP.Application.AccountsPayable.Payments.Contracts;

public sealed class ApPaymentAllocationInputDto
{
    public Guid ApInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }
}
