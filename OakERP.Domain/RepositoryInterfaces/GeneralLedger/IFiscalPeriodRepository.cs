using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.RepositoryInterfaces.GeneralLedger;

public interface IFiscalPeriodRepository
{
    ValueTask<FiscalPeriod?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<FiscalPeriod?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<FiscalPeriod?> GetOpenForDateAsync(DateOnly postingDate, CancellationToken ct = default);

    IQueryable<FiscalPeriod> QueryNoTracking();

    Task AddAsync(FiscalPeriod entity);

    Task RemoveAsync(FiscalPeriod entity);
}
