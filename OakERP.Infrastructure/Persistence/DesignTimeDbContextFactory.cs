using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OakERP.Common.Enums;

namespace OakERP.Infrastructure.Persistence
{
    public sealed class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var projectDir = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
            );

            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            cfgBuilder.AddUserSecrets<ApplicationDbContext>(optional: true);

            var config = cfgBuilder.Build();

            var cs =
                config.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(
                    cs,
                    npgsql =>
                        npgsql
                            .MapEnum<DocStatus>("doc_status")
                            .MapEnum<GlAccountType>("gl_account_type")
                            .MapEnum<ItemType>("item_type")
                            .MapEnum<InventoryTransactionType>("inventory_transaction_type")
                )
                .UseSnakeCaseNamingConvention()
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
