using OakERP.Common.Enums;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Entities.Accounts_Payable;

public sealed class ApPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocNo { get; set; } = default!;
    public Guid VendorId { get; set; }
    public Guid BankAccountId { get; set; }

    /// <summary>
    /// check/transfer/value date (can be future)
    /// </summary>
    public DateOnly PaymentDate { get; set; }

    /// <summary>
    /// GL posting date (required when Posted)
    /// </summary>
    public DateOnly? PostingDate { get; set; }

    /// <summary>
    /// set by bank reconciliation (optional)
    /// </summary>
    public DateOnly? ClearedDate { get; set; }

    public decimal Amount { get; set; }
    public DocStatus DocStatus { get; set; } = DocStatus.Draft;
    public string? Memo { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public Vendor Vendor { get; set; } = default!;
    public BankAccount BankAccount { get; set; } = default!;

    public ICollection<ApPaymentAllocation> Allocations { get; set; } = [];
}