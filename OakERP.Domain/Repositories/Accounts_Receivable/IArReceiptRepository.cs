using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repositories.Accounts_Receivable;

public interface IArReceiptRepository
{
    Task<ArReceipt?> GetByIdAsync(Guid id);

    IQueryable<ArReceipt> Query();

    Task CreateAsync(ArReceipt arReceipt);

    Task UpdateAsync(ArReceipt arReceipt);

    Task DeleteAsync(ArReceipt arReceipt);
}
