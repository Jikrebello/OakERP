using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlEntryRepository(ApplicationDbContext db) : IGlEntryRepository
{
    private DbSet<GlEntry> Set => db.GlEntries;

    public Task<GlEntry?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<GlEntry?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<GlEntry> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(GlEntry entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(GlEntry entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}