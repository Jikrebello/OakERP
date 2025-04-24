using OakERP.Domain.Entities;

namespace OakERP.Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);

    Task<Tenant?> GetByNameAsync(string name);

    Task<IEnumerable<Tenant>> GetAllAsync();

    Task CreateAsync(Tenant tenant);

    Task UpdateAsync(Tenant tenant);

    Task DeleteAsync(Tenant tenant);
}