using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Domain.Entities.Accounts_Payable;

public sealed class ApInvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApInvoiceId { get; set; }
    public int LineNo { get; set; }
    public string? Description { get; set; }
    public string? AccountNo { get; set; } // expense account
    public Guid? ItemId { get; set; }
    public decimal Qty { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public Guid? TaxRateId { get; set; }
    public decimal LineTotal { get; set; }

    public ApInvoice Invoice { get; set; } = default!;
    public GlAccount? Account { get; set; }
    public TaxRate? TaxRate { get; set; }
    public Item? Item { get; set; }
}
