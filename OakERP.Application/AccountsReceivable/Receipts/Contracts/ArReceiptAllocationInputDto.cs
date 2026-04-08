namespace OakERP.Application.AccountsReceivable.Receipts.Contracts;

public sealed class ArReceiptAllocationInputDto
{
    public Guid ArInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }
}
