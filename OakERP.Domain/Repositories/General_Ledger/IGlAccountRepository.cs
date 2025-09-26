using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repositories.General_Ledger;

public interface IGlAccountRepository
{
    Task<GlAccount?> GetByIdAsync(Guid id);

    IQueryable<GlAccount> Query();

    Task CreateAsync(GlAccount glAccount);

    Task UpdateAsync(GlAccount glAccount);

    Task DeleteAsync(GlAccount glAccount);
}
