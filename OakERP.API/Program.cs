using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OakERP.Application.Extensions;
using OakERP.API.Extensions;
using OakERP.API.Runtime;
using OakERP.Auth.Extensions;
using OakERP.Infrastructure.Extensions;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;
using OakERP.Infrastructure.Persistence.Seeding.Accounts;
using OakERP.Infrastructure.Persistence.Seeding.Views;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(
    (context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "OakERP.API")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}"
            );

        var seqLoggingSettings = SeqLoggingSettings.Bind(context.Configuration);
        if (seqLoggingSettings.Enabled)
        {
            loggerConfiguration.WriteTo.Seq(
                seqLoggingSettings.ServerUrl!,
                apiKey: seqLoggingSettings.GetApiKeyOrNull()
            );
        }
    },
    writeToProviders: true
);

var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("Cors:AllowedOrigins is not configured.");

// Services
builder.Services.AddControllers();
builder.Services.AddRuntimeSupport(builder.Configuration);

builder
    .Services.AddApplicationDb(builder.Configuration)
    .AddIdentityServices()
    .AddApplicationServices()
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

app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("live") }
    )
    .DisableRequestTimeout()
    .AllowAnonymous();
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("ready") }
    )
    .DisableRequestTimeout()
    .AllowAnonymous();

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
