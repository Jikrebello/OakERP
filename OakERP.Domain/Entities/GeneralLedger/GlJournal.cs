using OakERP.Common.Enums;

namespace OakERP.Domain.Entities.GeneralLedger;

public sealed class GlJournal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string JournalNo { get; set; } = default!;
    public DateOnly JournalDate { get; set; }

    public DateOnly? PostingDate { get; set; }
    public Guid? PeriodId { get; set; }

    public DocStatus DocStatus { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public FiscalPeriod? Period { get; set; }
    public ICollection<GlJournalLine> Lines { get; set; } = [];
}
