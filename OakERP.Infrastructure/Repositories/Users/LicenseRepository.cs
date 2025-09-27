using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repositories.Users;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Users;

public class LicenseRepository(ApplicationDbContext db) : ILicenseRepository
{
    public async Task<License?> GetByIdAsync(Guid id) =>
        await db.Licenses.Include(l => l.Tenant).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<License?> GetByTenantIdAsync(Guid tenantId) =>
        await db.Licenses.Include(l => l.Tenant).FirstOrDefaultAsync(l => l.TenantId == tenantId);

    public IQueryable<License> Query() => db.Licenses.Include(l => l.Tenant).AsNoTracking();

    public async Task CreateAsync(License license)
    {
        db.Licenses.Add(license);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(License license)
    {
        db.Licenses.Update(license);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(License license)
    {
        db.Licenses.Remove(license);
        await db.SaveChangesAsync();
    }
}