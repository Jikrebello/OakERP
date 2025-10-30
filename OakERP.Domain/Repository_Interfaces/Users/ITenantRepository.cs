using OakERP.Domain.Entities.Users;

namespace OakERP.Domain.Repository_Interfaces.Users;

public interface ITenantRepository
{
    ValueTask<Tenant?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<Tenant?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<Tenant> QueryNoTracking();

    void Add(Tenant entity);

    void Remove(Tenant entity);
}