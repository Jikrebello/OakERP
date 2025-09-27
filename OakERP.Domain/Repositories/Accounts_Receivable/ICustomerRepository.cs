using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repositories.Accounts_Receivable;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);

    IQueryable<Customer> Query();

    Task CreateAsync(Customer customer);

    Task UpdateAsync(Customer customer);

    Task DeleteAsync(Customer customer);
}
