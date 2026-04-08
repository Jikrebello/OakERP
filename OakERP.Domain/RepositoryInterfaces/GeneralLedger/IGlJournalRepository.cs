using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.RepositoryInterfaces.GeneralLedger;

public interface IGlJournalRepository
{
    ValueTask<GlJournal?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<GlJournal?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<GlJournal> QueryNoTracking();

    Task AddAsync(GlJournal entity);

    Task RemoveAsync(GlJournal entity);
}
