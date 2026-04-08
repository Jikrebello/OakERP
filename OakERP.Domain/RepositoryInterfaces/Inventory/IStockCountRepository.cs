using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.RepositoryInterfaces.Inventory;

public interface IStockCountRepository
{
    ValueTask<StockCount?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<StockCount?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<StockCount> QueryNoTracking();

    Task AddAsync(StockCount entity);

    Task RemoveAsync(StockCount entity);
}
