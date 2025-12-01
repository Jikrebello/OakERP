using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArReceiptAllocationRepository(ApplicationDbContext db) : IArReceiptAllocationRepository
{
    private DbSet<ArReceiptAllocation> Set => db.ArReceiptAllocations;

    public Task<ArReceiptAllocation?> FindNoTrackingAsync(
        Guid id,
        CancellationToken ct = default
    ) => Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ArReceiptAllocation?> FindTrackedAsync(
        Guid id,
        CancellationToken ct = default
    ) => Set.FindAsync([id], ct);

    public IQueryable<ArReceiptAllocation> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ArReceiptAllocation entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ArReceiptAllocation entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
