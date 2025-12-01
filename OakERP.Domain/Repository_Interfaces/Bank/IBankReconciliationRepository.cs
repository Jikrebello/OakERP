using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Repository_Interfaces.Bank;

public interface IBankReconciliationRepository
{
    ValueTask<BankReconciliation?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<BankReconciliation?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<BankReconciliation> QueryNoTracking();

    Task AddAsync(BankReconciliation entity);

    Task RemoveAsync(BankReconciliation entity);
}
