using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.Entities.Inventory;

public sealed class ItemCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = default!; // e.g., "RAW", "FG", "SERV"
    public string Name { get; set; } = default!;

    public string? InventoryAccount { get; set; } // FK → GlAccount.AccountNo
    public string? CogsAccount { get; set; } // FK → GlAccount.AccountNo
    public string? AdjustAccount { get; set; } // FK → GlAccount.AccountNo
    public string? RevenueAccount { get; set; } // FK → GlAccount.AccountNo (optional fallback for AR)

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GlAccount? InvAccount { get; set; }
    public GlAccount? Cogs { get; set; }
    public GlAccount? Adjust { get; set; }
    public GlAccount? Revenue { get; set; }

    public ICollection<Item> Items { get; set; } = [];
}
