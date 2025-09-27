using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repositories.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class StockCountRepository(ApplicationDbContext db) : IStockCountRepository
{
    public async Task<StockCount?> GetByIdAsync(Guid id) =>
        await db.StockCounts.FirstOrDefaultAsync(sc => sc.Id == id);

    public IQueryable<StockCount> Query() => db.StockCounts.AsNoTracking();

    public async Task CreateAsync(StockCount stockCount)
    {
        db.StockCounts.Add(stockCount);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(StockCount stockCount)
    {
        db.StockCounts.Update(stockCount);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(StockCount stockCount)
    {
        db.StockCounts.Remove(stockCount);
        await db.SaveChangesAsync();
    }
}