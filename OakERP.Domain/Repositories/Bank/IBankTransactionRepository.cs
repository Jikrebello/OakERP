using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Repositories.Bank;

public interface IBankTransactionRepository
{
    Task<BankTransaction?> GetByIdAsync(Guid id);

    IQueryable<BankTransaction> Query();

    Task CreateAsync(BankTransaction bankTransaction);

    Task UpdateAsync(BankTransaction bankTransaction);

    Task DeleteAsync(BankTransaction bankTransaction);
}
