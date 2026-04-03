namespace OakERP.API.Extensions;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var inboundCorrelationId = context
            .Request.Headers[HttpContextExtensions.CorrelationHeaderName]
            .FirstOrDefault();

        var correlationId =
            !string.IsNullOrWhiteSpace(inboundCorrelationId) && inboundCorrelationId.Length <= 128
                ? inboundCorrelationId
                : Guid.NewGuid().ToString("N");

        context.SetCorrelationId(correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HttpContextExtensions.CorrelationHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        return next(context);
    }
}
