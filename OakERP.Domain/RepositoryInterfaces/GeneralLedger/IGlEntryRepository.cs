using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.RepositoryInterfaces.GeneralLedger;

public interface IGlEntryRepository
{
    ValueTask<GlEntry?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<GlEntry?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<GlEntry> QueryNoTracking();

    Task AddAsync(GlEntry entity);

    Task RemoveAsync(GlEntry entity);
}
