namespace OakERP.Domain.Entities.General_Ledger;

public sealed class GlEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly EntryDate { get; set; }
    public Guid PeriodId { get; set; }
    public string AccountNo { get; set; } = default!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public string SourceType { get; set; } = default!;
    public Guid SourceId { get; set; }
    public string? SourceNo { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public FiscalPeriod Period { get; set; } = default!;
    public GlAccount Account { get; set; } = default!;
}
