using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Repository_Interfaces.Bank;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Bank;

public class BankReconciliationRepository(ApplicationDbContext db) : IBankReconciliationRepository
{
    private DbSet<BankReconciliation> Set => db.BankReconciliations;

    public Task<BankReconciliation?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<BankReconciliation?> FindTrackedAsync(
        Guid id,
        CancellationToken ct = default
    ) => Set.FindAsync([id], ct);

    public IQueryable<BankReconciliation> QueryNoTracking() => Set.AsNoTracking();

    public void Add(BankReconciliation entity) => Set.Add(entity);

    public void Remove(BankReconciliation entity) => Set.Remove(entity);
}