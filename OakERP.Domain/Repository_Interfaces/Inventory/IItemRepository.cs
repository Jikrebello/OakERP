using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repository_Interfaces.Inventory;

public interface IItemRepository
{
    ValueTask<Item?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Item?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Item> QueryNoTracking();

    void Add(Item entity);

    void Remove(Item entity);
}