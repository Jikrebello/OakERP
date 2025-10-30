using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class ItemCategoryRepository(ApplicationDbContext db) : IItemCategoryRepository
{
    private DbSet<ItemCategory> Set => db.ItemCategories;

    public ValueTask<ItemCategory?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<ItemCategory?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<ItemCategory> QueryNoTracking() => Set.AsNoTracking();

    public void Add(ItemCategory entity) => Set.Add(entity);

    public void Remove(ItemCategory entity) => Set.Remove(entity);
}