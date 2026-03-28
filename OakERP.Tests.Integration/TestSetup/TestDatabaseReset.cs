using Npgsql;
using Respawn;

namespace OakERP.Tests.Integration.TestSetup;

public static class TestDatabaseReset
{
    private static Respawner? _respawner;

    public static async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(TestConfiguration.GetResetConnectionString());
        await conn.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(
            conn,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = [new("__EFMigrationsHistory")],
            }
        );

        await _respawner.ResetAsync(conn);
    }
}
