using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repositories.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class ItemRepository(ApplicationDbContext db) : IItemRepository
{
    public async Task<Item?> GetByIdAsync(Guid id) =>
        await db.Items.FirstOrDefaultAsync(i => i.Id == id);

    public IQueryable<Item> Query() => db.Items.AsNoTracking();

    public async Task CreateAsync(Item item)
    {
        db.Items.Add(item);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Item item)
    {
        db.Items.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
    }
}