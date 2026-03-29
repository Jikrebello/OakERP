using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Domain.Posting.Accounts_Receivable;

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
