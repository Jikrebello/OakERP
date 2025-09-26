using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repositories.Inventory;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id);

    IQueryable<Location> Query();

    Task CreateAsync(Location location);

    Task UpdateAsync(Location location);

    Task DeleteAsync(Location location);
}
