using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting.AccountsReceivable;

public sealed record ArReceiptPostingContext(
    ArReceipt Receipt,
    DateOnly PostingDate,
    FiscalPeriod Period,
    GlPostingSettings Settings,
    PostingRule Rule,
    string BankAccountNo
);
