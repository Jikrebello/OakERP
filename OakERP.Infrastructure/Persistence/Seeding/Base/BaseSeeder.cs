namespace OakERP.Infrastructure.Persistence.Seeding.Base;

public abstract class BaseSeeder : ISeeder
{
    public abstract int Order { get; }

    protected virtual bool RunInDevelopment => true;
    protected virtual bool RunInTesting => true;
    protected virtual bool RunInProduction => false;

    public virtual bool IsEnabled(string environment)
    {
        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            return RunInDevelopment;

        if (environment.Equals("Testing", StringComparison.OrdinalIgnoreCase))
            return RunInTesting;

        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            return RunInProduction;

        // Unknown env → off by default
        return false;
    }

    public abstract Task SeedAsync();
}
