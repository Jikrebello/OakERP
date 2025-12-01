using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Receivable;

public interface IArInvoiceRepository
{
    ValueTask<ArInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArInvoice> QueryNoTracking();

    Task AddAsync(ArInvoice entity);

    Task RemoveAsync(ArInvoice entity);
}
