using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repositories.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class StockCountLineRepository(ApplicationDbContext db) : IStockCountLineRepository
{
    public async Task<StockCountLine?> GetByIdAsync(Guid id) =>
        await db.StockCountLines.FirstOrDefaultAsync(stl => stl.Id == id);

    public IQueryable<StockCountLine> Query() => db.StockCountLines.AsNoTracking();

    public async Task CreateAsync(StockCountLine stockCountLine)
    {
        db.StockCountLines.Add(stockCountLine);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(StockCountLine stockCountLine)
    {
        db.StockCountLines.Update(stockCountLine);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(StockCountLine stockCountLine)
    {
        db.StockCountLines.Remove(stockCountLine);
        await db.SaveChangesAsync();
    }
}