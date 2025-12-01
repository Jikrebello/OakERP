using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Receivable;

public interface IArInvoiceLineRepository
{
    ValueTask<ArInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArInvoiceLine> QueryNoTracking();

    Task AddAsync(ArInvoiceLine entity);

    Task RemoveAsync(ArInvoiceLine entity);
}
