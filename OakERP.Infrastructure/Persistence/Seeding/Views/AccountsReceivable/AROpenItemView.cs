namespace OakERP.Infrastructure.Persistence.Seeding.Views.AccountsReceivable;

public sealed class AROpenItemView
{
    public Guid InvoiceId { get; set; }
    public string DocNo { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal DocTotal { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal Balance { get; set; }
}
