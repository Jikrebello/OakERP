using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repository_Interfaces.General_Ledger;

public interface IGlEntryRepository
{
    ValueTask<GlEntry?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<GlEntry?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<GlEntry> QueryNoTracking();

    void Add(GlEntry entity);

    void Remove(GlEntry entity);
}
