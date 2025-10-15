using OakERP.Common.Enums;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;

namespace OakERP.Domain.Entities.Accounts_Receivable;

public sealed class ArReceipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocNo { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Guid BankAccountId { get; set; }

    // Business dates
    public DateOnly ReceiptDate { get; set; } // money received / value date (can be future)

    public DateOnly? PostingDate { get; set; } // GL posting date (required when Posted)
    public DateOnly? ClearedDate { get; set; } // bank reconciliation date (optional)

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = CurrencyISOCodes.ZAR.ToString();
    public decimal? AmountForeign { get; set; } // amount in receipt currency
    public decimal? ExchangeRate { get; set; } // to functional currency for posting
    public Currency Currency { get; set; } = default!;
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public Customer Customer { get; set; } = default!;
    public BankAccount BankAccount { get; set; } = default!;

    public ICollection<ArReceiptAllocation> Allocations { get; set; } = [];
}
