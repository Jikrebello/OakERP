using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.RepositoryInterfaces.GeneralLedger;

public interface IGlJournalLineRepository
{
    ValueTask<GlJournalLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<GlJournalLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<GlJournalLine> QueryNoTracking();

    Task AddAsync(GlJournalLine entity);

    Task RemoveAsync(GlJournalLine entity);
}
