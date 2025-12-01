using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repository_Interfaces.General_Ledger;

public interface IGlAccountRepository
{
    ValueTask<GlAccount?> FindTrackedAsync(string accountNo, CancellationToken ct = default);

    Task<GlAccount?> FindNoTrackingAsync(string accountNo, CancellationToken ct = default);

    IQueryable<GlAccount> QueryNoTracking();

    Task AddAsync(GlAccount entity);

    Task RemoveAsync(GlAccount entity);
}
