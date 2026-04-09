using System.Xml.Linq;
using OakERP.Domain.Entities.Users;
using Shouldly;

namespace OakERP.Tests.Unit.Architecture;

public sealed class DependencyRulesTests
{
    [Fact]
    public void Domain_Assembly_Should_Not_Reference_AspNetCore_Ef_Or_Outer_Layers()
    {
        string[] references =
        [
            .. typeof(Tenant)
                .Assembly.GetReferencedAssemblies()
                .Select(x => x.Name ?? string.Empty),
        ];

        references.ShouldNotContain(name =>
            name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
        );
        references.ShouldNotContain(name =>
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
        );
        references.ShouldNotContain(name => name.Equals("OakERP.API", StringComparison.Ordinal));
        references.ShouldNotContain(name =>
            name.Equals("OakERP.Infrastructure", StringComparison.Ordinal)
        );
    }

    [Fact]
    public void Application_Assembly_Should_Not_Reference_Infrastructure()
    {
        string[] references =
        [
            .. typeof(IApInvoiceService)
                .Assembly.GetReferencedAssemblies()
                .Select(x => x.Name ?? string.Empty),
        ];

        references.ShouldNotContain(name =>
            name.Equals("OakERP.Infrastructure", StringComparison.Ordinal)
        );
    }

    [Fact]
    public void Domain_Assembly_Should_Not_Define_ApplicationUser()
    {
        string[] exportedTypes =
        [
            .. typeof(Tenant).Assembly.GetExportedTypes().Select(x => x.FullName ?? string.Empty),
        ];

        exportedTypes.ShouldNotContain(name =>
            name.EndsWith(".ApplicationUser", StringComparison.Ordinal)
        );

        typeof(ApplicationUser).FullName.ShouldBe("OakERP.Auth.Identity.ApplicationUser");
    }

    [Fact]
    public void MigrationTool_Project_Should_Not_Reference_Api_Project()
    {
        XDocument project = XDocument.Load(
            Path.Combine(GetRepoRoot(), "OakERP.MigrationTool", "OakERP.MigrationTool.csproj")
        );

        string[] projectReferences =
        [
            .. project
                .Descendants("ProjectReference")
                .Select(x => (string?)x.Attribute("Include") ?? string.Empty),
        ];

        projectReferences.ShouldNotContain(path =>
            path.Contains("OakERP.API", StringComparison.OrdinalIgnoreCase)
        );
    }

    private static string GetRepoRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
