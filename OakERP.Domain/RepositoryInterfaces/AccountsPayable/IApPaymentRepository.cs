using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Domain.RepositoryInterfaces.AccountsPayable;

public interface IApPaymentRepository
{
    ValueTask<ApPayment?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApPayment?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsDocNoAsync(string docNo, CancellationToken ct = default);

    Task<ApPayment?> GetTrackedForAllocationAsync(Guid id, CancellationToken ct = default);

    Task<ApPayment?> GetTrackedForPostingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApPayment> QueryNoTracking();

    Task AddAsync(ApPayment entity);

    Task RemoveAsync(ApPayment entity);
}
