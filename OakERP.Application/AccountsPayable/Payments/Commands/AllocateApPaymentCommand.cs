using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsPayable.Payments.Commands;

public sealed class AllocateApPaymentCommand
{
    [JsonIgnore]
    public Guid PaymentId { get; set; }

    public DateOnly? AllocationDate { get; set; }
    public IReadOnlyList<ApPaymentAllocationInputDto> Allocations { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
