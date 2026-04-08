using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.AccountsReceivable;

public class CustomerRepository(ApplicationDbContext db) : ICustomerRepository
{
    private DbSet<Customer> Set => db.Customers;

    public Task<Customer?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<Customer?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<Customer> QueryNoTracking() => Set.AsNoTracking();

    public async Task AddAsync(Customer entity) => await Set.AddAsync(entity);

    public Task RemoveAsync(Customer entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}
