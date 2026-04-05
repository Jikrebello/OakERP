namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptAllocationInputDTO
{
    public Guid ArInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }
}
