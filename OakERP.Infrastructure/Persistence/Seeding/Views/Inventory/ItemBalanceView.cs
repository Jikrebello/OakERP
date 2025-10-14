namespace OakERP.Infrastructure.Persistence.Seeding.Views.Inventory;

public sealed class ItemBalanceView
{
    public Guid ItemId { get; set; }
    public Guid LocationId { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal ValueOnHand { get; set; }
    public decimal MovingAvgCost { get; set; }
}
