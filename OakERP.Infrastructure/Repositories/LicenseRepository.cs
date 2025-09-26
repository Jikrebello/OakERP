using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repositories;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories;

/// <summary>
/// Provides methods for managing and retrieving license data from the database.
/// </summary>
/// <remarks>This repository is responsible for performing CRUD operations on licenses, including retrieving
/// licenses by their unique identifier or associated tenant, as well as retrieving all licenses. It interacts with the
/// database context to persist changes.</remarks>
/// <param name="db"></param>
public class LicenseRepository(ApplicationDbContext db) : ILicenseRepository
{
    public async Task<License?> GetByIdAsync(Guid id) =>
        await db.Licenses.Include(l => l.Tenant).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<License?> GetByTenantIdAsync(Guid tenantId) =>
        await db.Licenses.Include(l => l.Tenant).FirstOrDefaultAsync(l => l.TenantId == tenantId);

    public async Task<IEnumerable<License>> GetAllAsync() =>
        await db.Licenses.Include(l => l.Tenant).ToListAsync();

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
