using OakERP.Common.Enums;

namespace OakERP.Application.Posting.Contracts;

public sealed record UnpostResult(
    DocKind DocKind,
    Guid SourceId,
    string SourceNo,
    int GlEntriesRemoved,
    int InventoryEntriesRemoved
);
