using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsPayable;

public class VendorRepository(ApplicationDbContext db) : IVendorRepository
{
    private DbSet<Vendor> Set => db.Vendors;

    public Task<Vendor?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<Vendor?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<Vendor> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(Vendor entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(Vendor entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
