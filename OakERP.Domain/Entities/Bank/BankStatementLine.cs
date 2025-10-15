using OakERP.Common.Enums;

namespace OakERP.Domain.Entities.Bank;

public sealed class BankStatementLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BankStatementId { get; set; }
    public Guid BankAccountId { get; set; } // duplicate for fast filtering

    public DateOnly TxnDate { get; set; }
    public decimal Amount { get; set; } // sign per statement convention
    public string? Description { get; set; }
    public string? Reference { get; set; } // check no/reference
    public string? Counterparty { get; set; } // payee/payer if present
    public string? ExternalLineId { get; set; } // unique id in file/feed if provided
    public string? RawCode { get; set; } // bank's transaction code

    // Matching
    public Guid? BankTransactionId { get; set; } // matched internal txn

    public string MatchStatus { get; set; } = BankStatementLineMatchStatus.Unmatched.ToString();

    public BankStatement Statement { get; set; } = default!;
    public BankAccount BankAccount { get; set; } = default!;
    public BankTransaction? BankTransaction { get; set; }
}
