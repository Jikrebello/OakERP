using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.RepositoryInterfaces.Inventory;
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

    public async Task AddAsync(InventoryLedger entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(InventoryLedger entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
