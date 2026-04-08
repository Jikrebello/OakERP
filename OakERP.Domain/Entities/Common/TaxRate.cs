using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.Entities.Common;

public sealed class TaxRate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// e.g. "VAT 15%", "Input VAT 15%", "Zero-rated"
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Store as a percent (e.g. 15.000 = 15%)
    /// </summary>
    public decimal RatePercent { get; set; }

    /// <summary>
    /// Purchases (Input VAT) vs Sales (Output VAT)
    /// </summary>
    public bool IsInput { get; set; }

    /// <summary>
    /// Optional effective window (allows rate changes over time)
    /// </summary>
    public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public ICollection<ApInvoiceLine> ApInvoiceLines { get; set; } = [];

    public ICollection<ArInvoiceLine> ArInvoiceLines { get; set; } = [];
}
