using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApInvoiceRepository(ApplicationDbContext db) : IApInvoiceRepository
{
    private DbSet<ApInvoice> Set => db.ApInvoices;

    public Task<ApInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ApInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

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
