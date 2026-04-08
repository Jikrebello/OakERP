using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.GeneralLedger;

public class GlAccountRepository(ApplicationDbContext db) : IGlAccountRepository
{
    private DbSet<GlAccount> Set => db.GlAccounts;

    public Task<GlAccount?> FindNoTrackingAsync(string accountNo, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.AccountNo == accountNo, ct);

    public ValueTask<GlAccount?> FindTrackedAsync(
        string accountNo,
        CancellationToken ct = default
    ) => Set.FindAsync([accountNo, ct], cancellationToken: ct);

    public IQueryable<GlAccount> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(GlAccount entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(GlAccount entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
