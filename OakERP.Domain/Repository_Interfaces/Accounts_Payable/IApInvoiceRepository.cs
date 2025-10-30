using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IApInvoiceRepository
{
    ValueTask<ApInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApInvoice> QueryNoTracking();

    void Add(ApInvoice entity);

    void Remove(ApInvoice entity);
}