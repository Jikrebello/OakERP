using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OakERP.API.Extensions;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.Sources.Clear();
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<ApplicationDbContext>(optional: true);
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
            var cs =
                ctx.Configuration.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true";

            services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(cs));

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
var db = sp.GetRequiredService<ApplicationDbContext>();

await db.Database.MigrateAsync();

var coordinator = sp.GetRequiredService<SeedCoordinator>();
await coordinator.RunAsync(environment);