using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repository_Interfaces.Inventory;

public interface IStockCountLineRepository
{
    ValueTask<StockCountLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<StockCountLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<StockCountLine> QueryNoTracking();

    Task AddAsync(StockCountLine entity);

    Task RemoveAsync(StockCountLine entity);
}
