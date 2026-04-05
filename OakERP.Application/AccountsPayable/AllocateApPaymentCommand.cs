using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsPayable;

public sealed class AllocateApPaymentCommand
{
    [JsonIgnore]
    public Guid PaymentId { get; set; }

    public DateOnly? AllocationDate { get; set; }
    public IReadOnlyList<ApPaymentAllocationInputDTO> Allocations { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
