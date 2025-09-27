using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Repositories.Bank;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Bank;

public class BankAccountRepository(ApplicationDbContext db) : IBankAccountRepository
{
    public async Task<BankAccount?> GetByIdAsync(Guid id) =>
        await db.BankAccounts.FirstOrDefaultAsync(a => a.Id == id);

    public IQueryable<BankAccount> Query() => db.BankAccounts.AsNoTracking();

    public async Task CreateAsync(BankAccount bankAccount)
    {
        db.BankAccounts.Add(bankAccount);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(BankAccount bankAccount)
    {
        db.BankAccounts.Update(bankAccount);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(BankAccount bankAccount)
    {
        db.BankAccounts.Remove(bankAccount);
        await db.SaveChangesAsync();
    }
}