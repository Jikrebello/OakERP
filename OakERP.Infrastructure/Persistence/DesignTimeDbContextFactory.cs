using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OakERP.Infrastructure.Persistence
{
    public sealed class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Point to the Infrastructure project folder (bin/.../net9.0 up 3 levels)
            var projectDir = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
            );

            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            try
            {
                cfgBuilder.AddUserSecrets<ApplicationDbContext>(optional: true);
            }
            catch { }

            var config = cfgBuilder.Build();

            var cs =
                config.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true";

            // TEMP: uncomment once to prove what it's reading
            //Console.WriteLine($"[EF] ConnectionString = {cs}");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(cs).Options;

            return new ApplicationDbContext(options);
        }
    }
}