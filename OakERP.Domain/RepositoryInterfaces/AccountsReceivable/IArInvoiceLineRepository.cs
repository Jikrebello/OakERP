using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

public interface IArInvoiceLineRepository
{
    ValueTask<ArInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArInvoiceLine> QueryNoTracking();

    Task AddAsync(ArInvoiceLine entity);

    Task RemoveAsync(ArInvoiceLine entity);
}
