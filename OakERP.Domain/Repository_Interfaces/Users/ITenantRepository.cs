using OakERP.Domain.Entities.Users;

namespace OakERP.Domain.Repository_Interfaces.Users;

public interface ITenantRepository
{
    ValueTask<Tenant?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Tenant?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Tenant> QueryNoTracking();

    Task AddAsync(Tenant entity);

    Task RemoveAsync(Tenant entity);
}
