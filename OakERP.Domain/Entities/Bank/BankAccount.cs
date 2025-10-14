using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Entities.Bank;

public sealed class BankAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string GlAccountNo { get; set; } = default!;
    public decimal OpeningBalance { get; set; }
    public string Currency { get; set; } = "BASE";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GlAccount GlAccount { get; set; } = default!;

    public ICollection<ApPayment> ApPayments { get; set; } = [];
}