using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class CustomerRepository(ApplicationDbContext db) : ICustomerRepository
{
    private DbSet<Customer> Set => db.Customers;

    public Task<Customer?> FindNoTrackingAsync(Guid id, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public ValueTask<Customer?> FindTrackedAsync(Guid id, CancellationToken ct = default) =>
        Set.FindAsync([id], ct);

    public IQueryable<Customer> QueryNoTracking() => Set.AsNoTracking();

    public void Add(Customer entity) => Set.Add(entity);

    public void Remove(Customer entity) => Set.Remove(entity);
}