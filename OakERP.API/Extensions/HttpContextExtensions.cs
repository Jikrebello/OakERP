namespace OakERP.API.Extensions;

public static class HttpContextExtensions
{
    public const string CorrelationHeaderName = "X-Correlation-ID";

    private const string CorrelationIdItemKey = "OakERP.Api.CorrelationId";

    public static string GetCorrelationId(this HttpContext context)
    {
        if (
            context.Items.TryGetValue(CorrelationIdItemKey, out var correlationId)
            && correlationId is string value
            && !string.IsNullOrWhiteSpace(value)
        )
        {
            return value;
        }

        return context.TraceIdentifier;
    }

    public static void SetCorrelationId(this HttpContext context, string correlationId)
    {
        context.Items[CorrelationIdItemKey] = correlationId;
    }
}
