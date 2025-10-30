using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

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

    public void Add(GlAccount entity) => Set.Add(entity);

    public void Remove(GlAccount entity) => Set.Remove(entity);
}
