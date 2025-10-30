using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlJournalLineRepository(ApplicationDbContext db) : IGlJournalLineRepository
{
    private DbSet<GlJournalLine> Set => db.GlJournalLines;

    public Task<GlJournalLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<GlJournalLine?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<GlJournalLine> QueryNoTracking() => Set.AsNoTracking();

    public void Add(GlJournalLine entity) => Set.Add(entity);

    public void Remove(GlJournalLine entity) => Set.Remove(entity);
}