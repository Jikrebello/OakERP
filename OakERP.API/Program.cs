using Microsoft.OpenApi.Models;
using OakERP.API.Extensions;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;
using OakERP.Infrastructure.Persistence.Seeding.Accounts;
using OakERP.Infrastructure.Persistence.Seeding.Views;

var builder = WebApplication.CreateBuilder(args);

// Controllers, swagger, etc.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "OakERP API", Version = "v1" });
});

// Modular registrations
builder
    .Services.AddApplicationDb(builder.Configuration)
    .AddPersistenceServices()
    .AddIdentityServices()
    .AddJwtAuth(builder.Configuration)
    .AddAuthServices()
    .AddSwaggerDocs();

// Register seeders discovered via reflection
builder.Services.AddSeedersFromAssemblies(
    typeof(RoleAndAdminSeeder).Assembly,
    typeof(SqlViewSeeder).Assembly
);
builder.Services.AddScoped<SeedCoordinator>();

builder.Services.AddScoped<DbInitializer>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "OakCors",
        policy =>
        {
            policy
                .WithOrigins("https://localhost:7094")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

var app = builder.Build();

// Middleware
app.UseCors("OakCors");
app.UseOakMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Seed on API startup (optional in prod)
var runSeedOnStartup = builder.Configuration.GetValue<bool?>("RunSeedOnStartup") ?? true;
if (runSeedOnStartup)
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<SeedCoordinator>();
    await seeder.RunAsync(app.Environment.EnvironmentName);
}

await app.RunAsync();

public partial class Program
{ }