using OakERP.Common.Enums;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Domain.Entities.Accounts_Recievable;

public sealed class ArReceipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocNo { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public decimal Amount { get; set; }
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
