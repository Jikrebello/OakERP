using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Entities.Inventory;

public sealed class ItemCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string? InventoryAccount { get; set; }
    public string? CogsAccount { get; set; }
    public string? AdjustAccount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GlAccount? InvAccount { get; set; }
    public GlAccount? Cogs { get; set; }
    public GlAccount? Adjust { get; set; }
}
