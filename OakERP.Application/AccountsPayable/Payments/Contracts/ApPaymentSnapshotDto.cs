using OakERP.Common.Enums;

namespace OakERP.Application.AccountsPayable.Payments.Contracts;

public sealed class ApPaymentSnapshotDto
{
    public Guid PaymentId { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public DocStatus DocStatus { get; set; }
    public string? Memo { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public IReadOnlyList<ApPaymentAllocationSnapshotDto> Allocations { get; set; } = [];
}
