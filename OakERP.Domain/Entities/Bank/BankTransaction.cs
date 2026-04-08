using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Domain.Entities.Bank;

public sealed class BankTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BankAccountId { get; set; }

    public DateOnly TxnDate { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount. Positive values indicate a deposit, while negative values indicate a
    /// withdrawal.
    /// </summary>
    public decimal Amount { get; set; } // +deposit, -withdrawal

    public string DrAccountNo { get; set; } = default!;
    public string CrAccountNo { get; set; } = default!;

    public string? SourceType { get; set; } // e.g., "ARReceipt","ApPayment","Journal"
    public Guid? SourceId { get; set; }
    public string? ExternalRef { get; set; } // bank statement ID / reference (optional)
    public string? Description { get; set; }

    public bool IsReconciled { get; set; }
    public DateOnly? ReconciledDate { get; set; } // set when reconciled

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public BankAccount BankAccount { get; set; } = default!;
    public GlAccount DrAccount { get; set; } = default!;
    public GlAccount CrAccount { get; set; } = default!;
}
