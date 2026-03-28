using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Tests.Integration.TestSetup;

public class OakErpWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            using var sp = services.BuildServiceProvider();
            var config = sp.GetRequiredService<IConfiguration>();

            var cs =
                config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not configured."
                );

            var toRemove = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
            );
            if (toRemove is not null)
                services.Remove(toRemove);

            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseNpgsql(cs).UseSnakeCaseNamingConvention()
            );
        });
    }
}
