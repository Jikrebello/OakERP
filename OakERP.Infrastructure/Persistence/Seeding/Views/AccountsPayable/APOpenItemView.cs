namespace OakERP.Infrastructure.Persistence.Seeding.Views.AccountsPayable;

public sealed class APOpenItemView
{
    public Guid InvoiceId { get; set; }
    public string DocNo { get; set; } = default!;
    public Guid VendorId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal DocTotal { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal Balance { get; set; }
}
