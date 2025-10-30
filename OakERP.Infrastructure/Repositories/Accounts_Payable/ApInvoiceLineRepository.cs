using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApInvoiceLineRepository(ApplicationDbContext db) : IApInvoiceLineRepository
{
    private DbSet<ApInvoiceLine> Set => db.ApInvoiceLines;

    public Task<ApInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ApInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ApInvoiceLine> QueryNoTracking() => Set.AsNoTracking();

    public void Add(ApInvoiceLine entity) => Set.Add(entity);

    public void Remove(ApInvoiceLine entity) => Set.Remove(entity);
}