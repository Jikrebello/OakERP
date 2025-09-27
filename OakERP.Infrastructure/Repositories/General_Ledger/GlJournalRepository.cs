using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repositories.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlJournalRepository(ApplicationDbContext db) : IGlJournalRepository
{
    public async Task<GlJournal?> GetByIdAsync(Guid id) =>
        await db.GlJournals.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<GlJournal> Query() => db.GlJournals.AsNoTracking();

    public async Task CreateAsync(GlJournal glJournal)
    {
        db.GlJournals.Add(glJournal);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(GlJournal glJournal)
    {
        db.GlJournals.Update(glJournal);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(GlJournal glJournal)
    {
        db.GlJournals.Remove(glJournal);
        await db.SaveChangesAsync();
    }
}