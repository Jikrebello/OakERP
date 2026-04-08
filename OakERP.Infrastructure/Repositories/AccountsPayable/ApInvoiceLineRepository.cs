using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsPayable;

public class ApInvoiceLineRepository(ApplicationDbContext db) : IApInvoiceLineRepository
{
    private DbSet<ApInvoiceLine> Set => db.ApInvoiceLines;

    public Task<ApInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ApInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ApInvoiceLine> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(ApInvoiceLine entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(ApInvoiceLine entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
