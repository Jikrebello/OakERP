using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Receivable;

public interface IArReceiptRepository
{
    ValueTask<ArReceipt?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ArReceipt?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default);

    Task<ArReceipt?> GetTrackedForAllocationAsync(Guid id, CancellationToken ct = default);

    Task<ArReceipt?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ArReceipt> QueryNoTracking();

    Task AddAsync(ArReceipt entity);

    Task RemoveAsync(ArReceipt entity);
}
