using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsPayable;

public interface IVendorRepository
{
    ValueTask<Vendor?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Vendor?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Vendor> QueryNoTracking();

    Task AddAsync(Vendor entity);

    Task RemoveAsync(Vendor entity);
}
