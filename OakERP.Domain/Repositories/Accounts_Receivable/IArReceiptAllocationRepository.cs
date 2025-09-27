using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repositories.Accounts_Receivable;

public interface IArReceiptAllocationRepository
{
    Task<ArReceiptAllocation?> GetByIdAsync(Guid id);

    IQueryable<ArReceiptAllocation> Query();

    Task CreateAsync(ArReceiptAllocation arReceiptAllocation);

    Task UpdateAsync(ArReceiptAllocation arReceiptAllocation);

    Task DeleteAsync(ArReceiptAllocation arReceiptAllocation);
}
