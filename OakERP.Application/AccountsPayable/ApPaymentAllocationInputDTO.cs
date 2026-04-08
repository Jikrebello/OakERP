namespace OakERP.Application.AccountsPayable;

public sealed class ApPaymentAllocationInputDto
{
    public Guid ApInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }
}
