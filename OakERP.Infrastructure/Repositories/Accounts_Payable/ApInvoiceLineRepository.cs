using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repositories.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApInvoiceLineRepository(ApplicationDbContext db) : IApInvoiceLineRepository
{
    public async Task<ApInvoiceLine?> GetByIdAsync(Guid id) =>
        await db.ApInvoiceLines.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<ApInvoiceLine> Query() => db.ApInvoiceLines.AsNoTracking();

    public async Task CreateAsync(ApInvoiceLine apInvoiceLine)
    {
        db.ApInvoiceLines.Add(apInvoiceLine);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApInvoiceLine apInvoiceLine)
    {
        db.ApInvoiceLines.Update(apInvoiceLine);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApInvoiceLine apInvoiceLine)
    {
        db.ApInvoiceLines.Remove(apInvoiceLine);
        await db.SaveChangesAsync();
    }
}