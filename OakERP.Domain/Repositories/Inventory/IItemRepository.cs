using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Repositories.Inventory;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id);

    IQueryable<Item> Query();

    Task CreateAsync(Item item);

    Task UpdateAsync(Item item);

    Task DeleteAsync(Item item);
}
