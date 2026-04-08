using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsPayable;

public interface IApPaymentAllocationRepository
{
    ValueTask<ApPaymentAllocation?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApPaymentAllocation?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApPaymentAllocation> QueryNoTracking();

    Task AddAsync(ApPaymentAllocation entity);

    Task RemoveAsync(ApPaymentAllocation entity);
}
