namespace OakERP.Domain.Posting.General_Ledger;

public sealed record GlEntryModel(
    DateOnly EntryDate,
    Guid PeriodId,
    string AccountNo,
    decimal Debit, // base-currency values
    decimal Credit,
    string SourceType, // e.g. "ARINV"
    Guid SourceId,
    string? SourceNo,
    string? Description
);