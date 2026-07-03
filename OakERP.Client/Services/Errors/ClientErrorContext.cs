namespace OakERP.Client.Services.Errors;

public sealed record ClientErrorContext(string Operation, string? Target = null);
