using Microsoft.EntityFrameworkCore;
using OakERP.Infrastructure.Persistence.Seeding.Base;
using static OakERP.Infrastructure.Persistence.SQL.Views.SqlViewManifest;

namespace OakERP.Infrastructure.Persistence.Seeding.Views;

/// <summary>
/// Recreates SQL views when their text changes (tracked by SHA-256).
/// </summary>
public sealed class SqlViewSeeder(ApplicationDbContext db) : BaseSeeder
{
    public override int Order => 50;
    protected override bool RunInProduction => true;

    public override async Task SeedAsync()
    {
        // 1) Ensure registry table exists
        await db.Database.ExecuteSqlRawAsync(
            """
                CREATE TABLE IF NOT EXISTS public.app_schema_object (
                  name       text PRIMARY KEY,
                  sha256     text NOT NULL,
                  updated_at timestamptz NOT NULL DEFAULT now()
                );
            """
        );

        // 2) Apply views that changed
        using var tx = await db.Database.BeginTransactionAsync();

        var asm = typeof(SqlViewSeeder).Assembly;

        foreach (var def in Discover(asm))
        {
            var createSql = ReadSql(asm, def.ResourcePath);
            var newHash = Sha256(createSql);

            // fetch existing hash (if any)
            var current = await db.Set<AppSchemaObject>()
                .FromSqlRaw(
                    """
                        SELECT name, sha256, updated_at
                        FROM public.app_schema_object
                        WHERE name = {0}
                    """,
                    def.Name
                )
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (!string.Equals(current?.Sha256, newHash, StringComparison.Ordinal))
            {
                // Drop old definition (if exists) and create the new one
                await db.Database.ExecuteSqlRawAsync(def.DropSql);
                await db.Database.ExecuteSqlRawAsync(createSql);

                // Upsert registry record
                await db.Database.ExecuteSqlRawAsync(
                    """
                        INSERT INTO public.app_schema_object(name, sha256)
                        VALUES ({0}, {1})
                        ON CONFLICT (name)
                        DO UPDATE SET sha256 = EXCLUDED.sha256, updated_at = now();
                    """,
                    def.Name,
                    newHash
                );
            }
        }

        await tx.CommitAsync();
    }

    // Minimal POCO to map SELECT results (not tracked by EF otherwise)
    private sealed class AppSchemaObject
    {
        public string Name { get; set; } = default!;
        public string Sha256 { get; set; } = default!;
        public DateTime Updated_At { get; set; }
    }
}