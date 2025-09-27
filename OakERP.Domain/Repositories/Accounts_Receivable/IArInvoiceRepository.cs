using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repositories.Accounts_Receivable;

public interface IArInvoiceRepository
{
    Task<ArInvoice?> GetByIdAsync(Guid id);

    IQueryable<ArInvoice> Query();

    Task CreateAsync(ArInvoice arInvoice);

    Task UpdateAsync(ArInvoice arInvoice);

    Task DeleteAsync(ArInvoice arInvoice);
}