using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repositories.Accounts_Payable;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(Guid id);

    IQueryable<Vendor> Query();

    Task CreateAsync(Vendor vendor);

    Task UpdateAsync(Vendor vendor);

    Task DeleteAsync(Vendor vendor);
}
