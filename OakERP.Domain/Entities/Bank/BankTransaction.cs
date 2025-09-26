using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Entities.Bank;

public sealed class BankTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BankAccountId { get; set; }
    public DateOnly TxnDate { get; set; }
    public decimal Amount { get; set; } // +deposit, -withdrawal
    public string DrAccountNo { get; set; } = default!;
    public string CrAccountNo { get; set; } = default!;
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? Description { get; set; }
    public bool IsReconciled { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public BankAccount BankAccount { get; set; } = default!;
    public GlAccount DrAccount { get; set; } = default!;
    public GlAccount CrAccount { get; set; } = default!;
}
