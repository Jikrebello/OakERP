using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.RepositoryInterfaces.Inventory;

public interface IItemRepository
{
    ValueTask<Item?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Item?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Item> QueryNoTracking();

    Task AddAsync(Item entity);

    Task RemoveAsync(Item entity);
}
