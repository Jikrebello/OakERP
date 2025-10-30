using Microsoft.Extensions.Logging;
using OakERP.Infrastructure.Persistence.Seeding.Base;

namespace OakERP.Infrastructure.Persistence.Seeding;

public sealed class SeedCoordinator(IEnumerable<ISeeder> seeders, ILogger<SeedCoordinator> logger)
{
    public async Task RunAsync(string environment)
    {
        var ordered = seeders.OrderBy(s => (s as BaseSeeder)?.Order ?? 0).ToList();
        if (ordered.Count == 0)
        {
            logger.LogInformation("No ISeeder implementations registered. Skipping seeding.");
            return;
        }

        foreach (var seeder in ordered)
        {
            var baseSeeder = seeder as BaseSeeder;
            var enabled = baseSeeder?.IsEnabled(environment) ?? true;

            if (!enabled)
            {
                logger.LogInformation(
                    "Skipping {Seeder} (disabled for {Env}).",
                    seeder.GetType().Name,
                    environment
                );
                continue;
            }

            logger.LogInformation("Running {Seeder}…", seeder.GetType().Name);
            await seeder.SeedAsync();
            logger.LogInformation("Finished {Seeder}.", seeder.GetType().Name);
        }
    }
}