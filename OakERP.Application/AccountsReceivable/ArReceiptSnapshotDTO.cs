using OakERP.Common.Enums;

namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptSnapshotDto
{
    public Guid ReceiptId { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DocStatus DocStatus { get; set; }
    public string? Memo { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public IReadOnlyList<ArReceiptAllocationSnapshotDto> Allocations { get; set; } = [];
}
