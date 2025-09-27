using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repositories.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArInvoiceLineRepository(ApplicationDbContext db) : IArInvoiceLineRepository
{
    public async Task<ArInvoiceLine?> GetByIdAsync(Guid id) =>
        await db.ArInvoiceLines.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<ArInvoiceLine> Query() => db.ArInvoiceLines.AsNoTracking();

    public async Task CreateAsync(ArInvoiceLine arInvoiceLine)
    {
        db.ArInvoiceLines.Add(arInvoiceLine);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ArInvoiceLine arInvoiceLine)
    {
        db.ArInvoiceLines.Update(arInvoiceLine);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ArInvoiceLine arInvoiceLine)
    {
        db.ArInvoiceLines.Remove(arInvoiceLine);
        await db.SaveChangesAsync();
    }
}