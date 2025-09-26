using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repositories.Accounts_Payable;

public interface IApPaymentAllocationRepository
{
    Task<ApPaymentAllocation?> GetByIdAsync(Guid id);

    IQueryable<ApPaymentAllocation> Query();

    Task CreateAsync(ApPaymentAllocation apPaymentAllocation);

    Task UpdateAsync(ApPaymentAllocation apPaymentAllocation);

    Task DeleteAsync(ApPaymentAllocation apPaymentAllocation);
}
