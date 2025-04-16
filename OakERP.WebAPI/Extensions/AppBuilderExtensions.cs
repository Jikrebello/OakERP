namespace OakERP.WebAPI.Extensions;

public static class AppBuilderExtensions
{
    public static WebApplication UseOakMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        // Add authentication, CORS, etc. later here
        return app;
    }
}
