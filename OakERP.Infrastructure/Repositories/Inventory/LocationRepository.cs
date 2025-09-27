using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repositories.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class LocationRepository(ApplicationDbContext db) : ILocationRepository
{
    public async Task<Location?> GetByIdAsync(Guid id) =>
        await db.Locations.FirstOrDefaultAsync(l => l.Id == id);

    public IQueryable<Location> Query() => db.Locations.AsNoTracking();

    public async Task CreateAsync(Location location)
    {
        db.Locations.Add(location);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Location location)
    {
        db.Locations.Update(location);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Location location)
    {
        db.Locations.Remove(location);
        await db.SaveChangesAsync();
    }
}