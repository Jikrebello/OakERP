namespace OakERP.Infrastructure.Persistence.Seeding;

public interface ISeeder
{
    Task SeedAsync();

    int Order { get; }

    bool IsEnabled(string environment);
}