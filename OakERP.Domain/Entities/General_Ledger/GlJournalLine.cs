namespace OakERP.Domain.Entities.General_Ledger;

public sealed class GlJournalLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JournalId { get; set; }
    public int LineNo { get; set; }
    public string AccountNo { get; set; } = default!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }

    public GlJournal Journal { get; set; } = default!;
    public GlAccount Account { get; set; } = default!;
}