using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

public interface IArReceiptAllocationRepository
{
    ValueTask<ArReceiptAllocation?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArReceiptAllocation?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArReceiptAllocation> QueryNoTracking();

    Task AddAsync(ArReceiptAllocation entity);

    Task RemoveAsync(ArReceiptAllocation entity);
}
