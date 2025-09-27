using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repositories.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Accounts_Receivable;

public class CustomerRepository(ApplicationDbContext db) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id) =>
        await db.Customers.FirstOrDefaultAsync(v => v.Id == id);

    public IQueryable<Customer> Query() => db.Customers.AsNoTracking();

    public async Task CreateAsync(Customer customer)
    {
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        db.Customers.Update(customer);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
    }
}