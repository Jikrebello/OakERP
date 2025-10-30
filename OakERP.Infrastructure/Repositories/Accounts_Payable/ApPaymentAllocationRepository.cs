using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

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

    public void Add(ApPaymentAllocation entity) => Set.Add(entity);

    public void Remove(ApPaymentAllocation entity) => Set.Remove(entity);
}