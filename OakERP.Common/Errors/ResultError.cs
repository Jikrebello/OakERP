namespace OakERP.Common.Errors;

public sealed record ResultError(string Code, string Message, FailureKind Kind);
