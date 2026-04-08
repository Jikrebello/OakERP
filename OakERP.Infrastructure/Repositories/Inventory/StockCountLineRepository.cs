using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.RepositoryInterfaces.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class StockCountLineRepository(ApplicationDbContext db) : IStockCountLineRepository
{
    private DbSet<StockCountLine> Set => db.StockCountLines;

    public ValueTask<StockCountLine?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<StockCountLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<StockCountLine> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(StockCountLine entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(StockCountLine entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
