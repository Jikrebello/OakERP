using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Entities.Common;

public sealed class Currency
{
    public string Code { get; set; } = default!;

    public short NumericCode { get; set; }

    public string Name { get; set; } = default!;
    public string? Symbol { get; set; }
    public short Decimals { get; set; } = 2;
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ApInvoice> ApInvoices { get; set; } = [];
    public ICollection<ArInvoice> ArInvoices { get; set; } = [];
    public ICollection<BankAccount> BankAccounts { get; set; } = [];
    public ICollection<ArReceipt> ArReceipts { get; set; } = [];
}