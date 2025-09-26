using OakERP.Domain.Entities.Users;

namespace OakERP.Domain.Repositories.Users;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);

    Task<Tenant?> GetByNameAsync(string name);

    IQueryable<Tenant> Query();

    Task CreateAsync(Tenant tenant);

    Task UpdateAsync(Tenant tenant);

    Task DeleteAsync(Tenant tenant);
}
