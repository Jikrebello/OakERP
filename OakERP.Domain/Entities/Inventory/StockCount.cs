using OakERP.Common.Enums;

namespace OakERP.Domain.Entities.Inventory;

public sealed class StockCount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CountNo { get; set; } = default!;
    public Guid LocationId { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public DateOnly ScheduledOn { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public Location Location { get; set; } = default!;
    public ICollection<StockCountLine> Lines { get; set; } = [];
}
