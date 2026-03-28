using OakERP.Common.Enums;

namespace OakERP.Application.Posting;

public sealed record PostResult(
    DocKind DocKind,
    Guid SourceId,
    string SourceNo,
    DateOnly PostingDate,
    Guid PeriodId,
    int GlEntryCount,
    int InventoryEntryCount
);