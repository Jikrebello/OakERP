using Microsoft.Extensions.Diagnostics.HealthChecks;
using OakERP.Infrastructure.Persistence;

namespace OakERP.API.Runtime;

public sealed class DatabaseConnectivityHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Database connectivity is available.")
                : HealthCheckResult.Unhealthy("Database connectivity is unavailable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connectivity check failed.",
                exception: ex
            );
        }
    }
}
