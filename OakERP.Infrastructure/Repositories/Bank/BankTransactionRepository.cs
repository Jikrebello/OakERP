using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Repositories.Bank;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Bank;

public class BankTransactionRepository(ApplicationDbContext db) : IBankTransactionRepository
{
    public async Task<BankTransaction?> GetByIdAsync(Guid id) =>
        await db.BankTransactions.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<BankTransaction> Query() => db.BankTransactions.AsNoTracking();

    public async Task CreateAsync(BankTransaction bankTransaction)
    {
        db.BankTransactions.Add(bankTransaction);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(BankTransaction bankTransaction)
    {
        db.BankTransactions.Update(bankTransaction);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(BankTransaction bankTransaction)
    {
        db.BankTransactions.Remove(bankTransaction);
        await db.SaveChangesAsync();
    }
}