using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class ItemRepository(ApplicationDbContext db) : IItemRepository
{
    private DbSet<Item> Set => db.Items;

    public ValueTask<Item?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<Item?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<Item> QueryNoTracking() => Set.AsNoTracking();

    public void Add(Item entity) => Set.Add(entity);

    public void Remove(Item entity) => Set.Remove(entity);
}