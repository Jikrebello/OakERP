using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsPayable;

public class ApPaymentRepository(ApplicationDbContext db) : IApPaymentRepository
{
    private DbSet<ApPayment> Set => db.ApPayments;

    public Task<ApPayment?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default) =>
        Set.AsNoTracking().AnyAsync(x => x.DocNo == docNo, ct);

    public Task<ApPayment?> GetTrackedForAllocationAsync(Guid id, CancellationToken ct = default) =>
        GetPostingQuery().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<ApPayment?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default) =>
        GetPostingQuery().SingleOrDefaultAsync(x => x.Id == id, ct);

    public ValueTask<ApPayment?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ApPayment> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ApPayment entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ApPayment entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }

    private IQueryable<ApPayment> GetPostingQuery() =>
        Set.Include(x => x.BankAccount).Include(x => x.Allocations);
}
