using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

public interface IArInvoiceRepository
{
    ValueTask<ArInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ArInvoice>> GetTrackedForAllocationAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default
    );

    Task<ArInvoice?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArInvoice> QueryNoTracking();

    Task AddAsync(ArInvoice entity);

    Task RemoveAsync(ArInvoice entity);
}
