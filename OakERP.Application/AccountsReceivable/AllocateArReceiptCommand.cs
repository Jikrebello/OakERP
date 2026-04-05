using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsReceivable;

public sealed class AllocateArReceiptCommand
{
    [JsonIgnore]
    public Guid ReceiptId { get; set; }

    public DateOnly? AllocationDate { get; set; }
    public IReadOnlyList<ArReceiptAllocationInputDTO> Allocations { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
