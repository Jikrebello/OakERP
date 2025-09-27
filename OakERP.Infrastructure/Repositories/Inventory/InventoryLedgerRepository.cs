using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repositories.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class InventoryLedgerRepository(ApplicationDbContext db) : IInventoryLedgerRepository
{
    public async Task<InventoryLedger?> GetByIdAsync(Guid id) =>
        await db.InventoryLedgers.FirstOrDefaultAsync(il => il.Id == id);

    public IQueryable<InventoryLedger> Query() => db.InventoryLedgers.AsNoTracking();

    public async Task CreateAsync(InventoryLedger inventoryLedger)
    {
        db.InventoryLedgers.Add(inventoryLedger);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InventoryLedger inventoryLedger)
    {
        db.InventoryLedgers.Update(inventoryLedger);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(InventoryLedger inventoryLedger)
    {
        db.InventoryLedgers.Remove(inventoryLedger);
        await db.SaveChangesAsync();
    }
}