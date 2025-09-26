using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repositories.Inventory;

public interface IStockCountLineRepository
{
    Task<StockCountLine?> GetByIdAsync(Guid id);

    IQueryable<StockCountLine> Query();

    Task CreateAsync(StockCountLine stockCountLine);

    Task UpdateAsync(StockCountLine stockCountLine);

    Task DeleteAsync(StockCountLine stockCountLine);
}
