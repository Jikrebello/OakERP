using OakERP.Domain.Entities.Users;

namespace OakERP.Domain.Repositories;

/// <summary>
/// Defines a repository for managing tenant entities, providing methods for retrieving, creating, updating, and
/// deleting tenants.
/// </summary>
/// <remarks>This interface abstracts the data access layer for tenant management, enabling operations such as
/// retrieving tenants by ID or name, fetching all tenants, and performing CRUD operations. Implementations of this
/// interface should ensure thread safety and handle persistence concerns appropriately.</remarks>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);

    Task<Tenant?> GetByNameAsync(string name);

    Task<IEnumerable<Tenant>> GetAllAsync();

    Task CreateAsync(Tenant tenant);

    Task UpdateAsync(Tenant tenant);

    Task DeleteAsync(Tenant tenant);
}
