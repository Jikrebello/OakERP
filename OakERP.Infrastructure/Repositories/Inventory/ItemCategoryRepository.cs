using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Repositories.Inventory;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Repositories.Inventory;

public class ItemCategoryRepository(ApplicationDbContext db) : IItemCategoryRepository
{
    public async Task<ItemCategory?> GetByIdAsync(Guid id) =>
        await db.ItemCategories.FirstOrDefaultAsync(ic => ic.Id == id);

    public IQueryable<ItemCategory> Query() => db.ItemCategories.AsNoTracking();

    public async Task CreateAsync(ItemCategory itemCategory)
    {
        db.ItemCategories.Add(itemCategory);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ItemCategory itemCategory)
    {
        db.ItemCategories.Update(itemCategory);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ItemCategory itemCategory)
    {
        db.ItemCategories.Remove(itemCategory);
        await db.SaveChangesAsync();
    }
}