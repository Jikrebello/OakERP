using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Payable;

public class ApPaymentRepository(ApplicationDbContext db) : IApPaymentRepository
{
    private DbSet<ApPayment> Set => db.ApPayments;

    public Task<ApPayment?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<ApPayment?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<ApPayment> QueryNoTracking() => Set.AsNoTracking();

    public void Add(ApPayment entity) => Set.Add(entity);

    public void Remove(ApPayment entity) => Set.Remove(entity);
}