namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptAllocationInputDto
{
    public Guid ArInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }
}
