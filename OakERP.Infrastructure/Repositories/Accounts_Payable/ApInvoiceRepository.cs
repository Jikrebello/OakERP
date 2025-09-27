using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repositories.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApInvoiceRepository(ApplicationDbContext db) : IApInvoiceRepository
{
    public async Task<ApInvoice?> GetByIdAsync(Guid id) =>
        await db.ApInvoices.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<ApInvoice> Query() => db.ApInvoices.AsNoTracking();

    public async Task CreateAsync(ApInvoice apInvoice)
    {
        db.ApInvoices.Add(apInvoice);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApInvoice apInvoice)
    {
        db.ApInvoices.Update(apInvoice);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApInvoice apInvoice)
    {
        db.ApInvoices.Remove(apInvoice);
        await db.SaveChangesAsync();
    }
}