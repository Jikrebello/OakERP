using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.Entities.GeneralLedger;

public sealed class GlAccount
{
    public string AccountNo { get; set; } = default!;
    public string Name { get; set; } = default!;
    public GlAccountType Type { get; set; }
    public string? ParentAccount { get; set; }
    public bool IsControl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GlAccount? Parent { get; set; }
    public ICollection<GlAccount> Children { get; set; } = [];

    // Back navigation
    public ICollection<GlEntry> Entries { get; set; } = [];

    public ICollection<GlJournalLine> JournalLines { get; set; } = [];
    public ICollection<ApInvoiceLine> ApInvoiceLines { get; set; } = [];
    public ICollection<ArInvoiceLine> ArInvoiceLines { get; set; } = [];
}
