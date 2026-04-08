using OakERP.Domain.Entities.Common;

namespace OakERP.Domain.RepositoryInterfaces.Common;

public interface ITaxRateRepository
{
    Task<TaxRate?> GetByIdAsync(Guid id);

    IQueryable<TaxRate> Query();

    Task CreateAsync(TaxRate taxRate);

    Task UpdateAsync(TaxRate taxRate);

    Task DeleteAsync(TaxRate taxRate);
}
