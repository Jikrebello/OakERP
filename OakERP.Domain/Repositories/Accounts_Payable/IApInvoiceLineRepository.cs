using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repositories.Accounts_Payable;

public interface IApInvoiceLineRepository
{
    Task<ApInvoiceLine?> GetByIdAsync(Guid id);

    IQueryable<ApInvoiceLine> Query();

    Task CreateAsync(ApInvoiceLine apInvoiceLine);

    Task UpdateAsync(ApInvoiceLine apInvoiceLine);

    Task DeleteAsync(ApInvoiceLine apInvoiceLine);
}
