namespace OakERP.Infrastructure.Persistence.Seeding;

public abstract class BaseSeeder : ISeeder
{
    public abstract int Order { get; }

    protected virtual bool RunInDevelopment => true;
    protected virtual bool RunInProduction => false;

    public virtual bool IsEnabled(string environment)
    {
        return (
                environment.Equals("Development", StringComparison.OrdinalIgnoreCase)
                && RunInDevelopment
            )
            || (
                environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
                && RunInProduction
            );
    }

    public abstract Task SeedAsync();
}