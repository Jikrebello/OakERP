namespace OakERP.Infrastructure.Persistence.Seeding.Base;

public abstract class BaseSeeder : ISeeder
{
    public abstract int Order { get; }

    protected virtual bool RunInDevelopment => true;
    protected virtual bool RunInProduction => false;

    public virtual bool IsEnabled(string environment) =>
        environment.Equals("Development", StringComparison.OrdinalIgnoreCase) && RunInDevelopment
        || environment.Equals("Production", StringComparison.OrdinalIgnoreCase) && RunInProduction;

    public abstract Task SeedAsync();
}
