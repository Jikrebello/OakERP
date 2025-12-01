using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repository_Interfaces.General_Ledger;

public interface IFiscalPeriodRepository
{
    ValueTask<FiscalPeriod?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<FiscalPeriod?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<FiscalPeriod> QueryNoTracking();

    Task AddAsync(FiscalPeriod entity);

    Task RemoveAsync(FiscalPeriod entity);
}
