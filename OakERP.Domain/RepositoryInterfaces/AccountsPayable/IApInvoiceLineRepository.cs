using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsPayable;

public interface IApInvoiceLineRepository
{
    ValueTask<ApInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApInvoiceLine> QueryNoTracking();

    Task AddAsync(ApInvoiceLine entity);

    Task RemoveAsync(ApInvoiceLine entity);
}
