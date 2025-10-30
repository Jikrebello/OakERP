using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Repository_Interfaces.Bank;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Bank;

public class BankAccountRepository(ApplicationDbContext db) : IBankAccountRepository
{
    private DbSet<BankAccount> Set => db.BankAccounts;

    public Task<BankAccount?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<BankAccount?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<BankAccount> QueryNoTracking() => Set.AsNoTracking();

    public void Add(BankAccount entity) => Set.Add(entity);

    public void Remove(BankAccount entity) => Set.Remove(entity);
}
