using OakERP.Domain.Shared.Enums;

namespace OakERP.Domain.Entities.General_Ledger;

public sealed class GlJournal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string JournalNo { get; set; } = default!;
    public DateOnly JournalDate { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }
    public ICollection<GlJournalLine> Lines { get; set; } = new List<GlJournalLine>();
}
