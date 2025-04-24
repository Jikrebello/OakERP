using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities;
using OakERP.Domain.Repositories;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories;

public class TenantRepository(ApplicationDbContext db) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id) =>
        await db.Tenants.Include(t => t.License).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tenant?> GetByNameAsync(string name) =>
        await db.Tenants.Include(t => t.License).FirstOrDefaultAsync(t => t.Name == name);

    public async Task<IEnumerable<Tenant>> GetAllAsync() =>
        await db.Tenants.Include(t => t.License).ToListAsync();

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