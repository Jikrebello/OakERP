using OakERP.Domain.Shared.Enums;

namespace OakERP.Domain.Entities.Inventory;

public sealed class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public ItemType Type { get; set; }
    public Guid? CategoryId { get; set; }
    public string Uom { get; set; } = "EA";
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ItemCategory? Category { get; set; }
}
