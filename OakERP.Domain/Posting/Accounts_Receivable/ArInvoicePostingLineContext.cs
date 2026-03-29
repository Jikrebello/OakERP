using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Domain.Posting.Accounts_Receivable;

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

