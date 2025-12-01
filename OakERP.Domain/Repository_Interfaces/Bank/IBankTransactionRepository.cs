using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Repository_Interfaces.Bank;

public interface IBankTransactionRepository
{
    ValueTask<BankTransaction?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<BankTransaction?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<BankTransaction> QueryNoTracking();

    Task AddAsync(BankTransaction entity);

    Task RemoveAsync(BankTransaction entity);
}
