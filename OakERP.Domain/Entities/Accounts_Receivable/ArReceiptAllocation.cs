namespace OakERP.Domain.Entities.Accounts_Receivable;

public sealed class ArReceiptAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ArReceiptId { get; set; }
    public Guid ArInvoiceId { get; set; }

    // Each row is a distinct allocation event
    public DateOnly AllocationDate { get; set; } // when this portion was applied

    public decimal AmountApplied { get; set; } // > 0

    public decimal? DiscountGiven { get; set; } // >= 0

    public decimal? WriteOffAmount { get; set; } // >= 0
    public string? Memo { get; set; }

    public ArReceipt Receipt { get; set; } = default!;
    public ArInvoice Invoice { get; set; } = default!;
}