using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class InventoryLedgerRepository(ApplicationDbContext db) : IInventoryLedgerRepository
{
    private DbSet<InventoryLedger> Set => db.InventoryLedgers;

    public ValueTask<InventoryLedger?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<InventoryLedger?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<InventoryLedger> QueryNoTracking() => Set.AsNoTracking();

    public void Add(InventoryLedger entity) => Set.Add(entity);

    public void Remove(InventoryLedger entity) => Set.Remove(entity);
}
