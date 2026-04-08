using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Common;

public class TaxRateRepository(ApplicationDbContext db) : ITaxRateRepository
{
    private DbSet<TaxRate> Set => db.TaxRates;

    public Task<TaxRate?> GetByIdAsync(Guid id) =>
        Set.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);

    public IQueryable<TaxRate> Query() => Set.AsNoTracking();

    public async Task CreateAsync(TaxRate taxRate) => await Set.AddAsync(taxRate);

    public Task UpdateAsync(TaxRate taxRate)
    {
        Set.Update(taxRate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaxRate taxRate)
    {
        Set.Remove(taxRate);
        return Task.CompletedTask;
    }
}
