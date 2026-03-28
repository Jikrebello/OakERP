using OakERP.Common.Enums;

namespace OakERP.Application.Posting;

public sealed record UnpostCommand(DocKind DocKind, Guid SourceId, string PerformedBy);