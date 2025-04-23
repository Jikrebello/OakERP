namespace OakERP.Tests.Integration.TestSetup;

[CollectionDefinition("TransactionalDB")]
public class TransactionalDbCollection : ICollectionFixture<TransactionalDbFixture>
{ }

[CollectionDefinition("PersistentDB")]
public class PersistentDbCollection : ICollectionFixture<PersistentDbFixture>
{ }

public class TransactionalDbFixture : IntegrationTestBase
{
    protected override bool UseTransaction => true;
}

public class PersistentDbFixture : IntegrationTestBase
{
    protected override bool UseTransaction => false;

    // Use HashSet to avoid duplicates and circular refs
    private readonly HashSet<object> _entitiesToCleanup = [];

    /// <summary>
    /// Register one or more entities for cleanup.
    /// All navigational children will be found and cleaned up automatically.
    /// </summary>
    public void RegisterEntitiesForCleanup(params object[] entities)
    {
        foreach (var entity in entities)
            AddEntityAndChildren(entity, _entitiesToCleanup);
    }

    /// <summary>
    /// Recursively adds all child entities first, then the parent, for correct cleanup order.
    /// </summary>
    private static void AddEntityAndChildren(object? entity, HashSet<object> visited)
    {
        if (entity is null)
            return;

        // Only add if not already processed (avoid circular references)
        if (!visited.Add(entity))
            return;

        var type = entity.GetType();

        // Look for all navigation properties (single or collection)
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

            // Handle collections (excluding string)
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

    public override async Task DisposeAsync()
    {
        // Remove all tracked entities in reverse order (children first)
        foreach (var entity in _entitiesToCleanup.Reverse())
        {
            if (!DbContext.ChangeTracker.Entries().Any(e => e.Entity == entity))
                DbContext.Attach(entity);

            DbContext.Remove(entity);
        }
        await DbContext.SaveChangesAsync();
        await base.DisposeAsync();
    }
}