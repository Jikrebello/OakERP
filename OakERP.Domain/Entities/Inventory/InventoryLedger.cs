using OakERP.Common.Enums;

namespace OakERP.Domain.Entities.Inventory;

public sealed class InventoryLedger
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly TrxDate { get; set; }
    public Guid ItemId { get; set; }
    public Guid LocationId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Qty { get; set; } // 18,4
    public decimal UnitCost { get; set; } // 18,4
    public decimal ValueChange { get; set; } // 18,2
    public string SourceType { get; set; } = default!;
    public Guid SourceId { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public Item Item { get; set; } = default!;
    public Location Location { get; set; } = default!;
}