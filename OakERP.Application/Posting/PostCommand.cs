using OakERP.Common.Enums;

namespace OakERP.Application.Posting;

public sealed record PostCommand(
    DocKind DocKind,
    Guid SourceId,
    string PerformedBy,
    DateOnly? PostingDate = null,
    bool Force = false
);