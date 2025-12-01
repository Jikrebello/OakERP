using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IApInvoiceLineRepository
{
    ValueTask<ApInvoiceLine?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApInvoiceLine?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApInvoiceLine> QueryNoTracking();

    Task AddAsync(ApInvoiceLine entity);

    Task RemoveAsync(ApInvoiceLine entity);
}
