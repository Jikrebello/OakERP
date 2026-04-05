using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IApPaymentRepository
{
    ValueTask<ApPayment?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApPayment?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default);

    Task<ApPayment?> GetTrackedForAllocationAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApPayment> QueryNoTracking();

    Task AddAsync(ApPayment entity);

    Task RemoveAsync(ApPayment entity);
}
