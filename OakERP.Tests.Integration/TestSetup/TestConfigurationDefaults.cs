namespace OakERP.Tests.Integration.TestSetup;

internal static class TestConfigurationDefaults
{
    public static IReadOnlyDictionary<string, string?> Values { get; } =
        new Dictionary<string, string?>
        {
            ["RunSeedOnStartup"] = "false",
            ["Serilog:Seq:Enabled"] = "false",
            ["Cors:AllowedOrigins:0"] = "https://localhost:7094",
            ["Seed:AdminEmail"] = "admin@oak.test",
            ["Seed:AdminPassword"] = "TestAdmin123!",
            ["JwtSettings:Key"] = "01234567890123456789012345678901",
            ["JwtSettings:Issuer"] = "OakERP",
            ["JwtSettings:Audience"] = "OakERPUsers",
            ["JwtSettings:ExpireMinutes"] = "60",
            ["ConnectionStrings:DefaultConnection"] =
                "Host=localhost;Port=5433;Database=oakerp_test;Username=oakadmin;Password=oakpass;Include Error Detail=true",
            ["ConnectionStrings:TransactionalConnection"] =
                "Host=localhost;Port=5433;Username=oakadmin;Password=oakpass;Database=oakerp",
            ["Tests:TransactionalConnectionString"] =
                "Host=localhost;Port=5433;Username=oakadmin;Password=oakpass;Database=oakerp",
            ["Tests:ResetConnectionString"] =
                "Host=localhost;Port=5433;Database=oakerp_test;Username=oakadmin;Password=oakpass;Include Error Detail=true",
        };
}
