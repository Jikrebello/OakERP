using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.Posting.Inventory;

namespace OakERP.Domain.Posting;

public sealed record PostingEngineResult(
    IReadOnlyList<GlEntryModel> GlEntries,
    IReadOnlyList<InventoryMovementModel> InventoryMovements
);
