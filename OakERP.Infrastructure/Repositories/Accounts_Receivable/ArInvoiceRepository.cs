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
