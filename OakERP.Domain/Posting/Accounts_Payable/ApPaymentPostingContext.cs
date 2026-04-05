using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Domain.Posting.Accounts_Payable;

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
