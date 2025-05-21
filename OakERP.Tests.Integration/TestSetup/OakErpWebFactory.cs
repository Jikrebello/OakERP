using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Tests.Integration.TestSetup;

/// <summary>
/// A factory for creating a test web application instance configured for integration testing with a PostgreSQL
/// database.
/// </summary>
/// <remarks>This factory customizes the web host by replacing the default database context configuration with a
/// PostgreSQL database connection. It is intended for use in integration tests where a real database is
/// required.</remarks>
public class OakErpWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
            );
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    "Host=localhost;Port=5432;Username=oakadmin;Password=oakpass;Database=oakerp"
                );
            });
        });
    }
}