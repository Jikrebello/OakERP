using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repositories.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class FiscalPeriodRepository(ApplicationDbContext db) : IFiscalPeriodRepository
{
    public async Task<FiscalPeriod?> GetByIdAsync(Guid id) =>
        await db.FiscalPeriods.FirstOrDefaultAsync(fp => fp.Id == id);

    public IQueryable<FiscalPeriod> Query() => db.FiscalPeriods.AsNoTracking();

    public async Task CreateAsync(FiscalPeriod fiscalPeriod)
    {
        db.FiscalPeriods.Add(fiscalPeriod);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(FiscalPeriod fiscalPeriod)
    {
        db.FiscalPeriods.Update(fiscalPeriod);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(FiscalPeriod fiscalPeriod)
    {
        db.FiscalPeriods.Remove(fiscalPeriod);
        await db.SaveChangesAsync();
    }
}