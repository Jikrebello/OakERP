using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repositories.Inventory;

public interface IStockCountRepository
{
    Task<StockCount?> GetByIdAsync(Guid id);

    IQueryable<StockCount> Query();

    Task CreateAsync(StockCount stockCount);

    Task UpdateAsync(StockCount stockCount);

    Task DeleteAsync(StockCount stockCount);
}
