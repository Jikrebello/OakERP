using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsPayable;

public class ApPaymentAllocationRepository(ApplicationDbContext db) : IApPaymentAllocationRepository
{
    private DbSet<ApPaymentAllocation> Set => db.ApPaymentAllocations;

    public Task<ApPaymentAllocation?> FindNoTrackingAsync(
        Guid id,
        CancellationToken ct = default
    ) => Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ApPaymentAllocation?> FindTrackedAsync(
        Guid id,
        CancellationToken ct = default
    ) => Set.FindAsync([id], ct);

    public IQueryable<ApPaymentAllocation> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ApPaymentAllocation entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ApPaymentAllocation entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
