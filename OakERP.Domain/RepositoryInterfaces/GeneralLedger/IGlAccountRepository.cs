using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.RepositoryInterfaces.GeneralLedger;

public interface IGlAccountRepository
{
    ValueTask<GlAccount?> FindTrackedAsync(string accountNo, CancellationToken ct = default);

    Task<GlAccount?> FindNoTrackingAsync(string accountNo, CancellationToken ct = default);

    IQueryable<GlAccount> QueryNoTracking();

    Task AddAsync(GlAccount entity);

    Task RemoveAsync(GlAccount entity);
}
