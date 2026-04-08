using Microsoft.Extensions.Configuration;

namespace OakERP.Tests.Integration.TestSetup;

internal static class TestConfiguration
{
    private static IConfigurationRoot BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables(prefix: "OakERP__")
            .AddEnvironmentVariables()
            .Build();

    public static OakErpTestDatabaseOptions GetDatabaseOptions()
    {
        IConfigurationRoot configuration = BuildConfiguration();
        var options = new OakErpTestDatabaseOptions
        {
            TransactionalConnectionString =
                configuration["Tests:TransactionalConnectionString"]
                ?? configuration.GetConnectionString("TransactionalConnection"),
            ResetConnectionString =
                configuration["Tests:ResetConnectionString"]
                ?? configuration.GetConnectionString("DefaultConnection"),
        };

        options.Validate();
        return options;
    }
}
