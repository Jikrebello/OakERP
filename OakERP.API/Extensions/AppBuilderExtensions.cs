namespace OakERP.API.Extensions;

public static class AppBuilderExtensions
{
    public static WebApplication UseOakMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseRouting();
        app.UseExceptionHandler();
        app.UseStatusCodePages(
            async context =>
                await Results.Problem(statusCode: context.HttpContext.Response.StatusCode)
                    .ExecuteAsync(context.HttpContext)
        );
        app.UseRateLimiter();
        app.UseRequestTimeouts();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
