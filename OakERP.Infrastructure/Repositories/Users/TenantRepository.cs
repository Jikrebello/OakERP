using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repository_Interfaces.Users;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Users;

public class TenantRepository(ApplicationDbContext db) : ITenantRepository
{
    private DbSet<Tenant> Set => db.Tenants;

    public ValueTask<Tenant?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<Tenant?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<Tenant> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(Tenant entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(Tenant entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
