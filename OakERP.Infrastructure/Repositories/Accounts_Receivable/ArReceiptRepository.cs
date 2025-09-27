using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repositories.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArReceiptRepository(ApplicationDbContext db) : IArReceiptRepository
{
    public async Task<ArReceipt?> GetByIdAsync(Guid id) =>
        await db.ArReceipts.FirstOrDefaultAsync(r => r.Id == id);

    public IQueryable<ArReceipt> Query() => db.ArReceipts.AsNoTracking();

    public async Task CreateAsync(ArReceipt arReceipt)
    {
        db.ArReceipts.Add(arReceipt);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ArReceipt arReceipt)
    {
        db.ArReceipts.Update(arReceipt);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ArReceipt arReceipt)
    {
        db.ArReceipts.Remove(arReceipt);
        await db.SaveChangesAsync();
    }
}