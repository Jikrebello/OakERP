using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Receivable;

public interface IArReceiptAllocationRepository
{
    ValueTask<ArReceiptAllocation?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArReceiptAllocation?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArReceiptAllocation> QueryNoTracking();

    void Add(ArReceiptAllocation entity);

    void Remove(ArReceiptAllocation entity);
}