using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.RepositoryInterfaces.Bank;

public interface IBankAccountRepository
{
    ValueTask<BankAccount?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<BankAccount?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<BankAccount> QueryNoTracking();

    Task AddAsync(BankAccount entity);

    Task RemoveAsync(BankAccount entity);
}
