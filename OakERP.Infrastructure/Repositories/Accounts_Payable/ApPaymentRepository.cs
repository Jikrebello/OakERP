using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repositories.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApPaymentRepository(ApplicationDbContext db) : IApPaymentRepository
{
    public async Task<ApPayment?> GetByIdAsync(Guid id) =>
        await db.ApPayments.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<ApPayment> Query() => db.ApPayments.AsNoTracking();

    public async Task CreateAsync(ApPayment apPayment)
    {
        db.ApPayments.Add(apPayment);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApPayment apPayment)
    {
        db.ApPayments.Update(apPayment);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApPayment apPayment)
    {
        db.ApPayments.Remove(apPayment);
        await db.SaveChangesAsync();
    }
}