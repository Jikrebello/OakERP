using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repository_Interfaces.Inventory;

public interface IInventoryLedgerRepository
{
    ValueTask<InventoryLedger?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<InventoryLedger?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<InventoryLedger> QueryNoTracking();

    void Add(InventoryLedger entity);

    void Remove(InventoryLedger entity);
}
