namespace OakERP.Infrastructure.Persistence.Seeding.Base;

public interface ISeeder
{
    Task SeedAsync();

    int Order { get; }

    bool IsEnabled(string environment);
}
