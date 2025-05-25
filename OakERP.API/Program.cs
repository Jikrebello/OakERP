using OakERP.API.Extensions;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;

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

builder.Services.AddScoped<ISeeder, RoleAndAdminSeeder>();
builder.Services.AddScoped<DbInitializer>();

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

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.SeedDbAsync();
}

await app.RunAsync();

public partial class Program
{ }