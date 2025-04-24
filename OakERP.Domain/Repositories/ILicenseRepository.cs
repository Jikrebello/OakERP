using OakERP.Domain.Entities;

namespace OakERP.Domain.Repositories;

public interface ILicenseRepository
{
    Task<License?> GetByTenantIdAsync(Guid tenantId);

    Task<License?> GetByIdAsync(Guid id);

    Task<IEnumerable<License>> GetAllAsync();

    Task CreateAsync(License license);

    Task UpdateAsync(License license);

    Task DeleteAsync(License license);
}