using OakERP.Domain.Entities.Accounts_Recievable;

namespace OakERP.Domain.Repositories.Accounts_Recievable;

public interface IArReceiptAllocationRepository
{
    Task<ArReceiptAllocation?> GetByIdAsync(Guid id);

    IQueryable<ArReceiptAllocation> Query();

    Task CreateAsync(ArReceiptAllocation arReceiptAllocation);

    Task UpdateAsync(ArReceiptAllocation arReceiptAllocation);

    Task DeleteAsync(ArReceiptAllocation arReceiptAllocation);
}
