using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Repository_Interfaces.Common;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Common;

public class CurrencyRepository(ApplicationDbContext db) : ICurrencyRepository
{
    private DbSet<Currency> Set => db.Currencies;

    public Task<Currency?> FindNoTrackingAsync(string code, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Code == code, ct);

    public ValueTask<Currency?> FindTrackedAsync(string code, CancellationToken ct = default) =>
        Set.FindAsync([code], ct);

    public IQueryable<Currency> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(Currency entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(Currency entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
