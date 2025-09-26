using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repositories.Accounts_Payable;

public interface IApPaymentRepository
{
    Task<ApPayment?> GetByIdAsync(Guid id);

    IQueryable<ApPayment> Query();

    Task CreateAsync(ApPayment apPayment);

    Task UpdateAsync(ApPayment apPayment);

    Task DeleteAsync(ApPayment apPayment);
}
