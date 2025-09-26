using OakERP.Domain.Entities.Accounts_Recievable;

namespace OakERP.Domain.Repositories.Accounts_Recievable;

public interface IArInvoiceRepository
{
    Task<ArInvoice?> GetByIdAsync(Guid id);

    IQueryable<ArInvoice> Query();

    Task CreateAsync(ArInvoice arInvoice);

    Task UpdateAsync(ArInvoice arInvoice);

    Task DeleteAsync(ArInvoice arInvoice);
}
