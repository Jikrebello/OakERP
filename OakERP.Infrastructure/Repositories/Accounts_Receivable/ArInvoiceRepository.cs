using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repositories.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArInvoiceRepository(ApplicationDbContext db) : IArInvoiceRepository
{
    public async Task<ArInvoice?> GetByIdAsync(Guid id) =>
        await db.ArInvoices.FirstOrDefaultAsync(i => i.Id == id);

    public IQueryable<ArInvoice> Query() => db.ArInvoices.AsNoTracking();

    public async Task CreateAsync(ArInvoice arInvoice)
    {
        db.ArInvoices.Add(arInvoice);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ArInvoice arInvoice)
    {
        db.ArInvoices.Update(arInvoice);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ArInvoice arInvoice)
    {
        db.ArInvoices.Remove(arInvoice);
        await db.SaveChangesAsync();
    }
}