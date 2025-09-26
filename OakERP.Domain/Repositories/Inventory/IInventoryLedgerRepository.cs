using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repositories.Inventory;

public interface IInventoryLedgerRepository
{
    Task<InventoryLedger?> GetByIdAsync(Guid id);

    IQueryable<InventoryLedger> Query();

    Task CreateAsync(InventoryLedger inventoryLedger);

    Task UpdateAsync(InventoryLedger inventoryLedger);

    Task DeleteAsync(InventoryLedger inventoryLedger);
}
