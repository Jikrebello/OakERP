using OakERP.API.Extensions;
using OakERP.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register all services (modular)
builder
    .Services.AddApplicationDb(builder.Configuration)
    .AddPersistenceServices()
    .AddIdentityServices()
    .AddJwtAuth(builder.Configuration)
    .AddAuthServices()
    .AddSwaggerDocs();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "OakCors",
        policy =>
        {
            policy
                .WithOrigins("https://localhost:7094") // Blazor Server UI
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

var app = builder.Build();

// Use middleware and tools (modular)
app.UseOakMiddleware();

await DbInitializer.SeedRolesAndAdminAsync(app.Services, app.Configuration);

app.Run();

public partial class Program
{ }