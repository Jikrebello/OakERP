using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class ArInvoiceLineRepository(ApplicationDbContext db) : IArInvoiceLineRepository
{
    private DbSet<ArInvoiceLine> Set => db.ArInvoiceLines;

    public Task<ArInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ArInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ArInvoiceLine> QueryNoTracking() => Set.AsNoTracking();

    public void Add(ArInvoiceLine entity) => Set.Add(entity);

    public void Remove(ArInvoiceLine entity) => Set.Remove(entity);
}