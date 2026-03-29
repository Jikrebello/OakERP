using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.General_Ledger;

public class FiscalPeriodRepository(ApplicationDbContext db) : IFiscalPeriodRepository
{
    private DbSet<FiscalPeriod> Set => db.FiscalPeriods;

    public ValueTask<FiscalPeriod?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public Task<FiscalPeriod?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<FiscalPeriod?> GetOpenForDateAsync(
        DateOnly postingDate,
        CancellationToken ct = default
    ) =>
        Set.AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.Status == FiscalPeriodStatuses.Open
                    && x.PeriodStart <= postingDate
                    && x.PeriodEnd >= postingDate,
                ct
            );

    public IQueryable<FiscalPeriod> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(FiscalPeriod entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(FiscalPeriod entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
