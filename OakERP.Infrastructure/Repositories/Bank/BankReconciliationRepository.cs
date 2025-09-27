using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Repositories.Bank;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Bank;

public class BankReconciliationRepository(ApplicationDbContext db) : IBankReconciliationRepository
{
    public async Task<BankReconciliation?> GetByIdAsync(Guid id) =>
        await db.BankReconciliations.FirstOrDefaultAsync(r => r.Id == id);

    public IQueryable<BankReconciliation> Query() => db.BankReconciliations.AsNoTracking();

    public async Task CreateAsync(BankReconciliation bankReconciliation)
    {
        db.BankReconciliations.Add(bankReconciliation);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(BankReconciliation bankReconciliation)
    {
        db.BankReconciliations.Update(bankReconciliation);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(BankReconciliation bankReconciliation)
    {
        db.BankReconciliations.Remove(bankReconciliation);
        await db.SaveChangesAsync();
    }
}