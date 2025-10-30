using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class StockCountRepository(ApplicationDbContext db) : IStockCountRepository
{
    private DbSet<StockCount> Set => db.StockCounts;

    public ValueTask<StockCount?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<StockCount?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<StockCount> QueryNoTracking() => Set.AsNoTracking();

    public void Add(StockCount entity) => Set.Add(entity);

    public void Remove(StockCount entity) => Set.Remove(entity);
}