using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Posting.Accounts_Payable;

public sealed record ApInvoicePostingLineContext(ApInvoiceLine Line, string ExpenseAccountNo);
