namespace OakERP.Client.Services.Errors;

public sealed record ClientErrorResult(string Message, int StatusCode = 500)
{
    public static ClientErrorResult Generic { get; } = new("An unexpected error occurred.", 500);
}
