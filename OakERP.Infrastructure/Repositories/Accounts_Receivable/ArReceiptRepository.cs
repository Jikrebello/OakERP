using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArReceiptRepository(ApplicationDbContext db) : IArReceiptRepository
{
    private DbSet<ArReceipt> Set => db.ArReceipts;

    public Task<ArReceipt?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default) =>
        Set.AsNoTracking().AnyAsync(x => x.DocNo == docNo, ct);

    public Task<ArReceipt?> GetTrackedForAllocationAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(x => x.Allocations).SingleOrDefaultAsync(x => x.Id == id, ct);

    public ValueTask<ArReceipt?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ArReceipt> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ArReceipt entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ArReceipt entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
