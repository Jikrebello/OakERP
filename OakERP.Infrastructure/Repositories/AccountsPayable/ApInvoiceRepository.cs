using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsPayable;

public class ApInvoiceRepository(ApplicationDbContext db) : IApInvoiceRepository
{
    private DbSet<ApInvoice> Set => db.ApInvoices;

    public Task<ApInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<ApInvoice>> GetTrackedForSettlementAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default
    ) =>
        ids.Count == 0
            ? []
            : await Set.Include(x => x.Allocations).Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public ValueTask<ApInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<ApInvoice?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, ct);

    public IQueryable<ApInvoice> QueryNoTracking() => Set.AsNoTracking();

    public Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default) =>
        Set.AsNoTracking().AnyAsync(x => x.DocNo == docNo, ct);

    public Task<bool> ExistsVendorInvoiceNoAsync(
        Guid vendorId,
        string invoiceNo,
        CancellationToken ct = default
    ) => Set.AsNoTracking().AnyAsync(x => x.VendorId == vendorId && x.InvoiceNo == invoiceNo, ct);

    public async Task AddAsync(ApInvoice entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ApInvoice entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
