using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repository_Interfaces.Inventory;

public interface IStockCountRepository
{
    ValueTask<StockCount?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<StockCount?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<StockCount> QueryNoTracking();

    void Add(StockCount entity);

    void Remove(StockCount entity);
}