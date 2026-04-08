using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.RepositoryInterfaces.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class LocationRepository(ApplicationDbContext db) : ILocationRepository
{
    private DbSet<Location> Set => db.Locations;

    public ValueTask<Location?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<Location?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<Location> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(Location entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(Location entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
