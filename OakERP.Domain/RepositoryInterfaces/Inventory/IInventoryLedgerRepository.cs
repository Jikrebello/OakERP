using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.RepositoryInterfaces.Inventory;

public interface IInventoryLedgerRepository
{
    ValueTask<InventoryLedger?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<InventoryLedger?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<InventoryLedger> QueryNoTracking();

    Task AddAsync(InventoryLedger entity);

    Task RemoveAsync(InventoryLedger entity);
}
