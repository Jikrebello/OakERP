using OakERP.Common.Enums;

namespace OakERP.Domain.Posting.Inventory;

public sealed record InventoryMovementModel(
    DateOnly TrxDate,
    Guid ItemId,
    Guid LocationId,
    InventoryTransactionType TransactionType,
    decimal Qty, // positive in, negative out
    decimal UnitCost, // base currency
    decimal ValueChange, // Qty * UnitCost, sign matches Qty
    string SourceType,
    Guid SourceId,
    string? Note
);
