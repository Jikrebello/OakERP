using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlJournalRepository(ApplicationDbContext db) : IGlJournalRepository
{
    private DbSet<GlJournal> Set => db.GlJournals;

    public Task<GlJournal?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<GlJournal?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<GlJournal> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(GlJournal entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(GlJournal entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
