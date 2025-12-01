using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repository_Interfaces.General_Ledger;

public interface IGlJournalLineRepository
{
    ValueTask<GlJournalLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<GlJournalLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<GlJournalLine> QueryNoTracking();

    Task AddAsync(GlJournalLine entity);

    Task RemoveAsync(GlJournalLine entity);
}
