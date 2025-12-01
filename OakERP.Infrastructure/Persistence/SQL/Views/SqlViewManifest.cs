using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace OakERP.Infrastructure.Persistence.SQL.Views;

public sealed record SqlViewDef(string Name, string ResourcePath, string DropSql);

public static class SqlViewManifest
{
    private const string Prefix = "OakERP.Infrastructure.Persistence.SQL.Views.";

    private const string Extension = ".psql";

    public static IEnumerable<SqlViewDef> Discover(Assembly asm)
    {
        foreach (
            var res in asm.GetManifestResourceNames()
                .Where(n =>
                    n.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
                    && n.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)
                )
        )
        {
            // Strip prefix and ".psql"
            var logical = res.Substring(
                Prefix.Length,
                res.Length - Prefix.Length - Extension.Length
            );

            var name = logical;

            var drop = $"DROP VIEW IF EXISTS {name};";

            yield return new SqlViewDef(name, res, drop);
        }
    }

    public static string ReadSql(Assembly asm, string resourcePath)
    {
        using var s =
            asm.GetManifestResourceStream(resourcePath)
            ?? throw new InvalidOperationException($"SQL resource not found: {resourcePath}");
        using var r = new StreamReader(s, Encoding.UTF8);
        return r.ReadToEnd();
    }

    public static string Sha256(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}