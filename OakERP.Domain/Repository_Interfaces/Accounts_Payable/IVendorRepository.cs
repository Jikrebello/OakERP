using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IVendorRepository
{
    ValueTask<Vendor?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Vendor?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Vendor> QueryNoTracking();

    void Add(Vendor entity);

    void Remove(Vendor entity);
}