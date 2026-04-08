using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.RepositoryInterfaces.Inventory;

public interface IItemCategoryRepository
{
    ValueTask<ItemCategory?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ItemCategory?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ItemCategory> QueryNoTracking();

    Task AddAsync(ItemCategory entity);

    Task RemoveAsync(ItemCategory entity);
}
