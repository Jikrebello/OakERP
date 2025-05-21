using Microsoft.EntityFrameworkCore;

namespace OakERP.Tests.Integration.TestSetup.Fixtures;

/// <summary>
/// Provides a fixture for integration tests that require persistent database operations.
/// </summary>
/// <remarks>This class disables transactional behavior by default, allowing tests to interact with the database
/// in a persistent manner. It also provides functionality to register entities for cleanup after tests are executed,
/// ensuring that the database state is reset.</remarks>
public class PersistentDbFixture : IntegrationTestBase
{
    protected override bool UseTransaction => false;

    private readonly HashSet<object> _entitiesToCleanup = [];

    // Call this in your test's [TearDown]
    public void RegisterEntitiesForCleanup(params object[] entities)
    {
        foreach (var entity in entities)
            AddEntityAndChildren(entity, _entitiesToCleanup);
    }

    private static void AddEntityAndChildren(object? entity, HashSet<object> visited)
    {
        if (entity is null)
            return;

        if (!visited.Add(entity))
            return;

        var type = entity.GetType();
        var navProps = type.GetProperties()
            .Where(p =>
                p.CanRead
                && !p.PropertyType.IsValueType
                && p.PropertyType != typeof(string)
                && (
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)
                    || p.PropertyType.Namespace == type.Namespace
                )
            )
            .ToList();

        foreach (var prop in navProps)
        {
            var value = prop.GetValue(entity);
            if (value is null)
                continue;

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                foreach (var child in enumerable)
                    AddEntityAndChildren(child, visited);
            }
            else
            {
                AddEntityAndChildren(value, visited);
            }
        }
    }

    public override async Task TearDown()
    {
        foreach (var entity in _entitiesToCleanup.Reverse())
        {
            if (!DbContext.ChangeTracker.Entries().Any(e => e.Entity == entity))
                DbContext.Attach(entity);

            DbContext.Remove(entity);
        }
        await DbContext.SaveChangesAsync();
        await base.TearDown();
    }

    public override async Task SetUp()
    {
        await base.SetUp();
    }
}