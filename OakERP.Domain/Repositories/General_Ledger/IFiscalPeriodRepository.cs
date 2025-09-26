using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repositories.General_Ledger;

public interface IFiscalPeriodRepository
{
    Task<FiscalPeriod?> GetByIdAsync(Guid id);

    IQueryable<FiscalPeriod> Query();

    Task CreateAsync(FiscalPeriod fiscalPeriod);

    Task UpdateAsync(FiscalPeriod fiscalPeriod);

    Task DeleteAsync(FiscalPeriod fiscalPeriod);
}
