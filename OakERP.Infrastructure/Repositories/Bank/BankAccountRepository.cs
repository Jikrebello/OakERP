using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.RepositoryInterfaces.Bank;
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

    public async Task AddAsync(BankAccount entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(BankAccount entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
