using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.Posting.AccountsReceivable;

public sealed record ArInvoicePostingLineContext(
    ArInvoiceLine Line,
    bool IsStock,
    string RevenueAccountNo,
    Guid? LocationId,
    string? CogsAccountNo,
    string? InventoryAssetAccountNo,
    decimal? UnitCost,
    decimal? LineCogsValue
);
