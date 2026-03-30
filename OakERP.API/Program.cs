using OakERP.API.Extensions;
using OakERP.Auth.Extensions;
using OakERP.Infrastructure.Extensions;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;
using OakERP.Infrastructure.Persistence.Seeding.Accounts;
using OakERP.Infrastructure.Persistence.Seeding.Views;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("Cors:AllowedOrigins is not configured.");

// Services
builder.Services.AddControllers();
builder.Services.AddRuntimeSupport();

builder
    .Services.AddApplicationDb(builder.Configuration)
    .AddIdentityServices()
    .AddJwtAuth(builder.Configuration)
    .AddAuthServices()
    .AddSwaggerDocs()
    .AddPersistenceServices()
    .AddRepositories()
    .AddPostingServices();

// Seeders
builder.Services.AddSeedersFromAssemblies(
    typeof(RoleAndAdminSeeder).Assembly,
    typeof(SqlViewSeeder).Assembly
);
builder.Services.AddScoped<SeedCoordinator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "OakCors",
        policy =>
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

var app = builder.Build();

// ----- HTTP pipeline -----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("OakCors");

app.UseOakMiddleware();

app.MapControllers();

// ----- Seeding (controlled by config; default is false in appsettings.json) -----
var runSeedOnStartup = builder.Configuration.GetValue<bool>("RunSeedOnStartup");
if (runSeedOnStartup)
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<SeedCoordinator>();
    await seeder.RunAsync(app.Environment.EnvironmentName);
}

await app.RunAsync();

public partial class Program { }
