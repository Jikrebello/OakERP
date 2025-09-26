using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Repositories.Bank;

public interface IBankReconciliationRepository
{
    Task<BankReconciliation?> GetByIdAsync(Guid id);

    IQueryable<BankReconciliation> Query();

    Task CreateAsync(BankReconciliation bankReconciliation);

    Task UpdateAsync(BankReconciliation bankReconciliation);

    Task DeleteAsync(BankReconciliation bankReconciliation);
}
