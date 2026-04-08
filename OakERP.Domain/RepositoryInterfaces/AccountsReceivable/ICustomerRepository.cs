using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

public interface ICustomerRepository
{
    ValueTask<Customer?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Customer?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Customer> QueryNoTracking();

    Task AddAsync(Customer entity);

    Task RemoveAsync(Customer entity);
}
