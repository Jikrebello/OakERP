using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Common;

public class TaxRateRepository(ApplicationDbContext db) : ITaxRateRepository
{
    private DbSet<TaxRate> Set => db.TaxRates;

    public ValueTask<TaxRate?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<TaxRate?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<TaxRate> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(TaxRate entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(TaxRate entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
