using System.Diagnostics;
using System.Security.Claims;

namespace OakERP.API.Extensions;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.GetCorrelationId();
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["TraceId"] = traceId,
            }
        );

        await next(context);

        stopwatch.Stop();

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = context.User.FindFirst("tenantId")?.Value;

        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms for user {UserId} tenant {TenantId}",
            context.Request.Method,
            context.Request.Path.Value ?? "/",
            context.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds,
            userId,
            tenantId
        );
    }
}
