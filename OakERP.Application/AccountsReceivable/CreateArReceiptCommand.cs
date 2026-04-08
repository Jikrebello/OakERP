using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsReceivable;

public sealed class CreateArReceiptCommand
{
    public string DocNo { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public DateOnly? AllocationDate { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Memo { get; set; }
    public IReadOnlyList<ArReceiptAllocationInputDto> Allocations { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
