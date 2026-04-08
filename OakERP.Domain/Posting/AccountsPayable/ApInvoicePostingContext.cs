using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting.AccountsPayable;

public sealed record ApInvoicePostingContext(
    ApInvoice Invoice,
    IReadOnlyList<ApInvoicePostingLineContext> Lines,
    DateOnly PostingDate,
    FiscalPeriod Period,
    GlPostingSettings Settings,
    PostingRule Rule
);
