namespace OakERP.Application.AccountsPayable;

public sealed class ApPaymentAllocationInputDTO
{
    public Guid ApInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }
}
