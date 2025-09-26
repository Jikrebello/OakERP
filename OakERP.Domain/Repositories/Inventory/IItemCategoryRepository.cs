using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repositories.Inventory;

public interface IItemCategoryRepository
{
    Task<ItemCategory?> GetByIdAsync(Guid id);

    IQueryable<ItemCategory> Query();

    Task CreateAsync(ItemCategory itemCategory);

    Task UpdateAsync(ItemCategory itemCategory);

    Task DeleteAsync(ItemCategory itemCategory);
}
