using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArInvoiceRepository(ApplicationDbContext db) : IArInvoiceRepository
{
    private DbSet<ArInvoice> Set => db.ArInvoices;

    public Task<ArInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<ArInvoice>> GetTrackedForAllocationAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default
    )
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await Set.Where(x => ids.Contains(x.Id)).Include(x => x.Allocations).ToListAsync(ct);
    }

    public Task<ArInvoice?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(x => x.Lines)
                .ThenInclude(x => x.Item)
                    .ThenInclude(x => x!.Category)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Location)
            .Include(x => x.Lines)
                .ThenInclude(x => x.TaxRate)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

    public ValueTask<ArInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ArInvoice> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ArInvoice entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ArInvoice entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
