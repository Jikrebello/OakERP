using OakERP.Common.Enums;

namespace OakERP.Domain.Entities.Bank;

public sealed class BankStatement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BankAccountId { get; set; }

    public DateOnly StatementFrom { get; set; }
    public DateOnly StatementTo { get; set; }

    public string Source { get; set; } = BankStatementSource.Manual.ToString(); // "csv","ofx","mt940","api",…
    public string? ExternalId { get; set; } // bank’s statement id if present
    public string? FileName { get; set; } // stored file name/hash if kept
    public string? Notes { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ImportedBy { get; set; }

    public BankAccount BankAccount { get; set; } = default!;
    public ICollection<BankStatementLine> Lines { get; set; } = [];
}
