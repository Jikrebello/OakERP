using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repository_Interfaces.Users;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Users;

public class LicenseRepository(ApplicationDbContext db) : ILicenseRepository
{
    private DbSet<License> Set => db.Licenses;

    public ValueTask<License?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<License?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<License> QueryNoTracking() => Set.AsNoTracking();

    public void Add(License entity) => Set.Add(entity);

    public void Remove(License entity) => Set.Remove(entity);
}
