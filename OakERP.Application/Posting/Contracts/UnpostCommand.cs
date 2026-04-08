using OakERP.Common.Enums;

namespace OakERP.Application.Posting.Contracts;

public sealed record UnpostCommand(DocKind DocKind, Guid SourceId, string PerformedBy);
