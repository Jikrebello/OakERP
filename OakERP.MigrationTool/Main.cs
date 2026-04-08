using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OakERP.Infrastructure.Extensions;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.Sources.Clear();
        cfg.SetBasePath(AppContext.BaseDirectory);
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureLogging(lb =>
    {
        lb.ClearProviders();
        lb.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
        });
    })
    .ConfigureServices(
        (ctx, services) =>
        {
            services
                .AddApplicationDb(ctx.Configuration, o => o.UseNodaTime())
                .AddIdentityServices();

            // register seeders (same discovery you use in API)
            services.AddSeedersFromAssemblies(
                typeof(ApplicationDbContext).Assembly,
                Assembly.GetExecutingAssembly()
            );

            services.AddScoped<SeedCoordinator>();
        }
    )
    .Build();

using var scope = host.Services.CreateScope();
var sp = scope.ServiceProvider;

// 1) migrate
var db = sp.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync();

// 2) seed
var coordinator = sp.GetRequiredService<SeedCoordinator>();
await coordinator.RunAsync(environment);
