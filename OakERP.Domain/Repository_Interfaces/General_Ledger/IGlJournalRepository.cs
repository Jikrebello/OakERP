using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repository_Interfaces.General_Ledger;

public interface IGlJournalRepository
{
    ValueTask<GlJournal?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<GlJournal?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<GlJournal> QueryNoTracking();

    void Add(GlJournal entity);

    void Remove(GlJournal entity);
}
