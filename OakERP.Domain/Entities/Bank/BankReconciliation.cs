namespace OakERP.Domain.Entities.Bank;

public sealed class BankReconciliation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BankAccountId { get; set; }

    public DateOnly StatementFrom { get; set; }
    public DateOnly StatementTo { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public BankAccount BankAccount { get; set; } = default!;
}
