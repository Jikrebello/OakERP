using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repositories.Accounts_Receivable;

public interface IArInvoiceLineRepository
{
    Task<ArInvoiceLine?> GetByIdAsync(Guid id);

    IQueryable<ArInvoiceLine> Query();

    Task CreateAsync(ArInvoiceLine arInvoiceLine);

    Task UpdateAsync(ArInvoiceLine arInvoiceLine);

    Task DeleteAsync(ArInvoiceLine arInvoiceLine);
}
