using OakERP.Domain.Entities.Common;

namespace OakERP.Domain.RepositoryInterfaces.Common;

public interface ITaxRateRepository
{
    ValueTask<TaxRate?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<TaxRate?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<TaxRate> QueryNoTracking();

    Task AddAsync(TaxRate entity);

    Task RemoveAsync(TaxRate entity);
}
