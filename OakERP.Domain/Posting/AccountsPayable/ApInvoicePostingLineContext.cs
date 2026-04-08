using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Domain.Posting.AccountsPayable;

public sealed record ApInvoicePostingLineContext(ApInvoiceLine Line, string ExpenseAccountNo);
