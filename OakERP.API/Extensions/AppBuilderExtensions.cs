namespace OakERP.API.Extensions;

public static class AppBuilderExtensions
{
    public static WebApplication UseOakMiddleware(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}