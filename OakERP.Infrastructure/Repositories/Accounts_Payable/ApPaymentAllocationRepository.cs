using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repositories.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApPaymentAllocationRepository(ApplicationDbContext db) : IApPaymentAllocationRepository
{
    public async Task<ApPaymentAllocation?> GetByIdAsync(Guid id) =>
        await db.ApPaymentAllocations.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<ApPaymentAllocation> Query() => db.ApPaymentAllocations.AsNoTracking();

    public async Task CreateAsync(ApPaymentAllocation apPaymentAllocation)
    {
        db.ApPaymentAllocations.Add(apPaymentAllocation);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApPaymentAllocation apPaymentAllocation)
    {
        db.ApPaymentAllocations.Update(apPaymentAllocation);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApPaymentAllocation apPaymentAllocation)
    {
        db.ApPaymentAllocations.Remove(apPaymentAllocation);
        await db.SaveChangesAsync();
    }
}