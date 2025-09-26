namespace OakERP.Domain.Entities.Inventory;

public sealed class StockCountLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StockCountId { get; set; }
    public int LineNo { get; set; }
    public Guid ItemId { get; set; }
    public decimal ExpectedQty { get; set; }
    public decimal CountedQty { get; set; }
    public decimal VarianceQty { get; set; }

    public StockCount StockCount { get; set; } = default!;
    public Item Item { get; set; } = default!;
}
