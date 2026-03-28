using Microsoft.Extensions.Configuration;

namespace OakERP.Tests.Integration.TestSetup;

internal static class TestConfiguration
{
    private const string DefaultTransactionalConnectionString =
        "Host=localhost;Port=5433;Username=oakadmin;Password=oakpass;Database=oakerp";

    private const string DefaultResetConnectionString =
        "Host=localhost;Port=5433;Database=oakerp_test;Username=oakadmin;Password=oakpass;Include Error Detail=true";

    private static IConfigurationRoot BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

    public static string GetTransactionalConnectionString()
    {
        return Environment.GetEnvironmentVariable("OakERP__Tests__TransactionalConnectionString")
            ?? BuildConfiguration().GetConnectionString("TransactionalConnection")
            ?? DefaultTransactionalConnectionString;
    }

    public static string GetResetConnectionString()
    {
        return Environment.GetEnvironmentVariable("OakERP__Tests__ResetConnectionString")
            ?? BuildConfiguration().GetConnectionString("DefaultConnection")
            ?? DefaultResetConnectionString;
    }
}
