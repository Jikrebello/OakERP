using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repository_Interfaces.Inventory;

public interface ILocationRepository
{
    ValueTask<Location?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Location?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Location> QueryNoTracking();

    Task AddAsync(Location entity);

    Task RemoveAsync(Location entity);
}
