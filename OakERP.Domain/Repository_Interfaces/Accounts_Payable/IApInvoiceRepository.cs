using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IApInvoiceRepository
{
    ValueTask<ApInvoice?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApInvoice?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ApInvoice>> GetTrackedForSettlementAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default
    );

    Task<ApInvoice?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApInvoice> QueryNoTracking();

    Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default);

    Task<bool> ExistsVendorInvoiceNoAsync(
        Guid vendorId,
        string invoiceNo,
        CancellationToken ct = default
    );

    Task AddAsync(ApInvoice entity);

    Task RemoveAsync(ApInvoice entity);
}
