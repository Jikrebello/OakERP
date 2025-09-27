using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repositories.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class VendorRepository(ApplicationDbContext db) : IVendorRepository
{
    public async Task<Vendor?> GetByIdAsync(Guid id) =>
        await db.Vendors.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<Vendor> Query() => db.Vendors.AsNoTracking();

    public async Task CreateAsync(Vendor vendor)
    {
        db.Vendors.Add(vendor);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        db.Vendors.Update(vendor);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vendor vendor)
    {
        db.Vendors.Remove(vendor);
        await db.SaveChangesAsync();
    }
}