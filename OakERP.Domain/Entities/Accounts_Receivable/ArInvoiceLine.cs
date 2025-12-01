using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Entities.Accounts_Receivable;

public sealed class ArInvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArInvoiceId { get; set; }
    public int LineNo { get; set; }

    public Guid? ItemId { get; set; } // nullable for service line
    public string? Description { get; set; }
    public decimal Qty { get; set; } = 1;
    public decimal UnitPrice { get; set; }

    public string? RevenueAccount { get; set; }
    public Guid? TaxRateId { get; set; }
    public decimal LineTotal { get; set; }

    public ArInvoice Invoice { get; set; } = default!;
    public Item? Item { get; set; }
    public GlAccount? Revenue { get; set; }
    public TaxRate? TaxRate { get; set; }
}
