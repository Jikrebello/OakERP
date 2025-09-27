using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repositories.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArReceiptAllocationRepository(ApplicationDbContext db) : IArReceiptAllocationRepository
{
    public async Task<ArReceiptAllocation?> GetByIdAsync(Guid id) =>
        await db.ArReceiptAllocations.FirstOrDefaultAsync(ra => ra.Id == id);

    public IQueryable<ArReceiptAllocation> Query() => db.ArReceiptAllocations.AsNoTracking();

    public async Task CreateAsync(ArReceiptAllocation arReceiptAllocation)
    {
        db.ArReceiptAllocations.Add(arReceiptAllocation);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ArReceiptAllocation arReceiptAllocation)
    {
        db.ArReceiptAllocations.Update(arReceiptAllocation);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ArReceiptAllocation arReceiptAllocation)
    {
        db.ArReceiptAllocations.Remove(arReceiptAllocation);
        await db.SaveChangesAsync();
    }
}