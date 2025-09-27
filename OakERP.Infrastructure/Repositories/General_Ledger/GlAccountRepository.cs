using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repositories.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class GlAccountRepository(ApplicationDbContext db) : IGlAccountRepository
{
    public async Task<GlAccount?> GetByAccountNoAsync(string accountNo) =>
        await db.GlAccounts.FirstOrDefaultAsync(a => a.AccountNo == accountNo);

    public IQueryable<GlAccount> Query() => db.GlAccounts.AsNoTracking();

    public async Task CreateAsync(GlAccount glAccount)
    {
        db.GlAccounts.Add(glAccount);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(GlAccount glAccount)
    {
        db.GlAccounts.Update(glAccount);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(GlAccount glAccount)
    {
        db.GlAccounts.Remove(glAccount);
        await db.SaveChangesAsync();
    }
}