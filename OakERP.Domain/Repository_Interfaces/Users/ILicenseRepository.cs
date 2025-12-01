using OakERP.Domain.Entities.Users;

namespace OakERP.Domain.Repository_Interfaces.Users;

public interface ILicenseRepository
{
    ValueTask<License?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<License?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<License> QueryNoTracking();

    Task AddAsync(License entity);

    Task RemoveAsync(License entity);
}
