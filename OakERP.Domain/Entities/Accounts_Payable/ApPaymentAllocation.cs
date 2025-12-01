namespace OakERP.Domain.Entities.Accounts_Payable;

public sealed class ApPaymentAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApPaymentId { get; set; }
    public Guid ApInvoiceId { get; set; }

    /// <summary>
    /// When this portion was applied
    /// </summary>
    public DateOnly AllocationDate { get; set; }

    public decimal AmountApplied { get; set; }

    /// <summary>
    /// Early payment discount per event
    /// </summary>
    public decimal? DiscountTaken { get; set; }

    /// <summary>
    /// Small balance write-off per event
    /// </summary>
    public decimal? WriteOffAmount { get; set; }

    public string? Memo { get; set; }

    public ApPayment Payment { get; set; } = default!;
    public ApInvoice Invoice { get; set; } = default!;
}
