using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Repository_Interfaces.Accounts_Payable;

public interface IApPaymentRepository
{
    ValueTask<ApPayment?> FindTrackedAsync(Guid id, CancellationToken ct = default);

    Task<ApPayment?> FindNoTrackingAsync(Guid id, CancellationToken ct = default);

    IQueryable<ApPayment> QueryNoTracking();

    void Add(ApPayment entity);

    void Remove(ApPayment entity);
}