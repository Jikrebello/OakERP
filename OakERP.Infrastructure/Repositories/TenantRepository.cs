using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repositories.Users;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories;

public class TenantRepository(ApplicationDbContext db) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id) =>
        await db.Tenants.Include(t => t.License).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tenant?> GetByNameAsync(string name) =>
        await db.Tenants.Include(t => t.License).FirstOrDefaultAsync(t => t.Name == name);

    public IQueryable<Tenant> Query() => db.Tenants.Include(t => t.License).AsNoTracking();

    public async Task CreateAsync(Tenant tenant)
    {
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        db.Tenants.Update(tenant);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Tenant tenant)
    {
        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync();
    }
}
