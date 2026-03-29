using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Entities.Inventory;

public sealed class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public ItemType Type { get; set; }

    public Guid? CategoryId { get; set; }
    public string Uom { get; set; } = "EA"; // unit-of-measure (code like EA, BOX, KG)

    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ItemCategory? Category { get; set; }

    public string? DefaultRevenueAccountNo { get; set; } // AR side fallback
    public string? DefaultExpenseAccountNo { get; set; } // AP side fallback

    public GlAccount? DefaultRevenueAccount { get; set; }
    public GlAccount? DefaultExpenseAccount { get; set; }

    public ICollection<ApInvoiceLine> ApInvoiceLines { get; set; } = [];
    public ICollection<ArInvoiceLine> ArInvoiceLines { get; set; } = [];
    public ICollection<InventoryLedger> InventoryLedgers { get; set; } = [];
}
