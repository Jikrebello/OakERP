using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting.AccountsPayable;

public sealed record ApPaymentPostingContext(
    ApPayment Payment,
    DateOnly PostingDate,
    FiscalPeriod Period,
    GlPostingSettings Settings,
    PostingRule Rule,
    string BankAccountNo,
    decimal AllocatedAmount,
    decimal UnappliedAmount
);
