using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Repository_Interfaces.Bank;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Bank;

public class BankTransactionRepository(ApplicationDbContext db) : IBankTransactionRepository
{
    private DbSet<BankTransaction> Set => db.BankTransactions;

    public Task<BankTransaction?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<BankTransaction?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<BankTransaction> QueryNoTracking() => Set.AsNoTracking();

    public void Add(BankTransaction entity) => Set.Add(entity);

    public void Remove(BankTransaction entity) => Set.Remove(entity);
}