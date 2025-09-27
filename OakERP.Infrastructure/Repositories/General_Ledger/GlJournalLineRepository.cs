using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repositories.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlJournalLineRepository(ApplicationDbContext db) : IGlJournalLineRepository
{
    public async Task<GlJournalLine?> GetByIdAsync(Guid id) =>
        await db.GlJournalLines.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<GlJournalLine> Query() => db.GlJournalLines.AsNoTracking();

    public async Task CreateAsync(GlJournalLine glJournalLine)
    {
        db.GlJournalLines.Add(glJournalLine);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(GlJournalLine glJournalLine)
    {
        db.GlJournalLines.Update(glJournalLine);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(GlJournalLine glJournalLine)
    {
        db.GlJournalLines.Remove(glJournalLine);
        await db.SaveChangesAsync();
    }
}