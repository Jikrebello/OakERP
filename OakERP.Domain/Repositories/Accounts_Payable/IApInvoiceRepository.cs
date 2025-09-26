using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repositories.Accounts_Payable;

public interface IApInvoiceRepository
{
    Task<ApInvoice?> GetByIdAsync(Guid id);

    IQueryable<ApInvoice> Query();

    Task CreateAsync(ApInvoice apInvoice);

    Task UpdateAsync(ApInvoice apInvoice);

    Task DeleteAsync(ApInvoice apInvoice);
}
