using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;

namespace OakERP.Domain.Posting;

public sealed record PostingEngineResult(
    IReadOnlyList<GlEntryModel> GlEntries,
    IReadOnlyList<InventoryMovementModel> InventoryMovements
);
