using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsReceivable;

public class ArInvoiceLineRepository(ApplicationDbContext db) : IArInvoiceLineRepository
{
    private DbSet<ArInvoiceLine> Set => db.ArInvoiceLines;

    public Task<ArInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ArInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ArInvoiceLine> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ArInvoiceLine entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ArInvoiceLine entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
