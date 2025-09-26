using OakERP.Domain.Entities.Users;

namespace OakERP.Domain.Repositories;

/// <summary>
/// Defines a contract for managing and retrieving license data in a persistent store.
/// </summary>
/// <remarks>This interface provides methods for common CRUD operations on licenses, including retrieving licenses
/// by tenant or ID, retrieving all licenses, and creating, updating, or deleting license records. Implementations of
/// this interface are responsible for handling the underlying data storage and retrieval logic.</remarks>
public interface ILicenseRepository
{
    Task<License?> GetByTenantIdAsync(Guid tenantId);

    Task<License?> GetByIdAsync(Guid id);

    Task<IEnumerable<License>> GetAllAsync();

    Task CreateAsync(License license);

    Task UpdateAsync(License license);

    Task DeleteAsync(License license);
}
