using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Repositories.Bank;

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(Guid id);

    IQueryable<BankAccount> Query();

    Task CreateAsync(BankAccount bankAccount);

    Task UpdateAsync(BankAccount bankAccount);

    Task DeleteAsync(BankAccount bankAccount);
}
