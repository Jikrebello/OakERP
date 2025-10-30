using OakERP.Domain.Entities.Common;

namespace OakERP.Domain.Repository_Interfaces.Common;

public interface ICurrencyRepository
{
    ValueTask<Currency?> FindTrackedAsync(string code, CancellationToken ct = default);

    Task<Currency?> FindNoTrackingAsync(string code, CancellationToken ct = default);

    IQueryable<Currency> QueryNoTracking();

    void Add(Currency entity);

    void Remove(Currency entity);
}
