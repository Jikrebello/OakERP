using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repositories.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlEntryRepository(ApplicationDbContext db) : IGlEntryRepository
{
    public async Task<GlEntry?> GetByIdAsync(Guid id) =>
        await db.GlEntries.FirstOrDefaultAsync(e => e.Id == id);

    public IQueryable<GlEntry> Query() => db.GlEntries.AsNoTracking();

    public async Task CreateAsync(GlEntry glEntry)
    {
        db.GlEntries.Add(glEntry);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(GlEntry glEntry)
    {
        db.GlEntries.Update(glEntry);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(GlEntry glEntry)
    {
        db.GlEntries.Remove(glEntry);
        await db.SaveChangesAsync();
    }
}