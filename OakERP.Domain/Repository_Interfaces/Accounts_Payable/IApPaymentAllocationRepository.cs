using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IApPaymentAllocationRepository
{
    ValueTask<ApPaymentAllocation?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApPaymentAllocation?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApPaymentAllocation> QueryNoTracking();

    Task AddAsync(ApPaymentAllocation entity);

    Task RemoveAsync(ApPaymentAllocation entity);
}
