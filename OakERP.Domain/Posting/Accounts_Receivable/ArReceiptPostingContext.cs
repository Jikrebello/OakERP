using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Domain.Posting.Accounts_Receivable;

public sealed record ArReceiptPostingContext(
    ArReceipt Receipt,
    DateOnly PostingDate,
    FiscalPeriod Period,
    string BaseCurrencyCode,
    decimal FxRateDocToBase,
    GlPostingSettings Settings,
    PostingRule Rule,
    string BankAccountNo,
    decimal AllocatedAmount,
    decimal UnappliedAmount
);
