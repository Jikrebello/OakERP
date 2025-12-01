using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OakERP.Infrastructure.Persistence.Seeding.Base;

namespace OakERP.Infrastructure.Persistence;

public class DbInitializer(
    IEnumerable<ISeeder> seeders,
    ILogger<DbInitializer> logger,
    IHostEnvironment hostEnv
)
{
    private readonly IEnumerable<ISeeder> _seeders =
    [
        .. seeders.Where(s => s.IsEnabled(hostEnv.EnvironmentName)).OrderBy(s => s.Order),
    ];

    private readonly string _environment = hostEnv.EnvironmentName;

    public async Task SeedDbAsync()
    {
        logger.LogInformation(
            "🌱 Starting database seed for {Environment} environment",
            _environment
        );

        foreach (var seeder in _seeders)
        {
            logger.LogInformation("🔸 Executing seeder: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync();
        }

        logger.LogInformation("✅ Seeding complete.");
    }
}
