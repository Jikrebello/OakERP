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

    public void Add(ApInvoice entity) => Set.Add(entity);

    public void Remove(ApInvoice entity) => Set.Remove(entity);
}