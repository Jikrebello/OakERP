using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting.AccountsReceivable;

public sealed record ArInvoicePostingContext(
    ArInvoice Invoice,
    IReadOnlyList<ArInvoicePostingLineContext> Lines,
    DateOnly PostingDate,
    FiscalPeriod Period,
    string BaseCurrencyCode,
    decimal FxRateDocToBase, // 1 doc-currency unit = X base-currency units
    GlPostingSettings Settings, // strongly-typed view over AppSettings (AR, revenue, inventory, tax accounts, etc.)
    PostingRule Rule // PostingRule for DocKind.ArInvoice
);
