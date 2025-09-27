namespace OakERP.Domain.Entities.Accounts_Receivable;

public sealed class ArReceiptAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArReceiptId { get; set; }
    public Guid ArInvoiceId { get; set; }
    public decimal AmountApplied { get; set; }

    public ArReceipt Receipt { get; set; } = default!;
    public ArInvoice Invoice { get; set; } = default!;
}
