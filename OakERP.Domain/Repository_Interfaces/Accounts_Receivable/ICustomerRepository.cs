using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Receivable;

public interface ICustomerRepository
{
    ValueTask<Customer?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Customer?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Customer> QueryNoTracking();

    Task AddAsync(Customer entity);

    Task RemoveAsync(Customer entity);
}
