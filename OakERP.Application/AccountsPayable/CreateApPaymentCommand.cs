using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsPayable;

public sealed class CreateApPaymentCommand
{
    public string DocNo { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateOnly PaymentDate { get; set; }
    public DateOnly? AllocationDate { get; set; }
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
    public IReadOnlyList<ApPaymentAllocationInputDto> Allocations { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
