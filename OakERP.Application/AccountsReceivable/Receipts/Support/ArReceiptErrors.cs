using OakERP.Common.Errors;
using OakERP.Application.Settlements.Documents;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

public static class ArReceiptErrors
{
    public static readonly ResultError DocumentNumberRequired =
        new("ar_receipt.document_number_required", "Document number is required.", FailureKind.Validation);
    public static readonly ResultError DocumentNumberTooLong =
        new("ar_receipt.document_number_too_long", "Document number may not exceed 40 characters.", FailureKind.Validation);
    public static readonly ResultError CustomerIdRequired =
        new("ar_receipt.customer_id_required", "Customer id is required.", FailureKind.Validation);
    public static readonly ResultError BankAccountIdRequired =
        new("ar_receipt.bank_account_id_required", "Bank account id is required.", FailureKind.Validation);
    public static readonly ResultError ReceiptAmountInvalid =
        new("ar_receipt.amount_invalid", "Receipt amount must be greater than zero.", FailureKind.Validation);
    public static readonly ResultError MemoTooLong =
        new("ar_receipt.memo_too_long", "Receipt memo may not exceed 512 characters.", FailureKind.Validation);
    public static readonly ResultError BaseCurrencyOnly =
        new("ar_receipt.base_currency_only", "AR receipts currently support only the base currency.", FailureKind.Validation);
    public static readonly ResultError ReceiptIdRequired =
        new("ar_receipt.receipt_id_required", "Receipt id is required.", FailureKind.Validation);
    public static readonly ResultError AllocationRequired =
        new("ar_receipt.allocation_required", SettlementDocumentErrorMessages.AllocationRequired, FailureKind.Validation);
    public static readonly ResultError AllocationInvoiceIdRequired =
        new("ar_receipt.allocation_invoice_id_required", "Allocation invoice id is required.", FailureKind.Validation);
    public static readonly ResultError AllocationDuplicateInvoice =
        new("ar_receipt.allocation_duplicate_invoice", "Each invoice may be allocated only once per request.", FailureKind.Validation);
    public static readonly ResultError AllocationAmountInvalid =
        new("ar_receipt.allocation_amount_invalid", "Allocation amount must be greater than zero.", FailureKind.Validation);
    public static readonly ResultError CustomerNotFound =
        new("ar_receipt.customer_not_found", "Customer was not found.", FailureKind.NotFound);
    public static readonly ResultError CustomerInactive =
        new("ar_receipt.customer_inactive", "AR receipts can be created only for active customers.", FailureKind.Validation);
    public static readonly ResultError BankAccountNotFound =
        new("ar_receipt.bank_account_not_found", SettlementDocumentErrorMessages.BankAccountNotFound, FailureKind.NotFound);
    public static readonly ResultError BankAccountInactive =
        new("ar_receipt.bank_account_inactive", "AR receipts can be created only for active bank accounts.", FailureKind.Validation);
    public static readonly ResultError ReceiptCurrencyMismatch =
        new("ar_receipt.currency_mismatch", "Receipt currency must match the selected bank account currency.", FailureKind.Validation);
    public static readonly ResultError DuplicateDocumentNumber =
        new("ar_receipt.duplicate_document_number", SettlementDocumentErrorMessages.DuplicateDocumentNumber, FailureKind.Conflict);
    public static readonly ResultError AllocationConcurrencyConflict =
        new("ar_receipt.allocation_concurrency_conflict", "The AR receipt was modified while allocations were being saved.", FailureKind.Conflict);
    public static readonly ResultError UnexpectedCreateFailure =
        new("ar_receipt.unexpected_create_failure", "Unexpected error while creating AR receipt.", FailureKind.Unexpected);
    public static readonly ResultError InvoicesNotFound =
        new("ar_receipt.invoices_not_found", "One or more AR invoices were not found.", FailureKind.NotFound);
    public static readonly ResultError OnlyPostedInvoicesAllowed =
        new("ar_receipt.only_posted_invoices_allowed", "Only posted AR invoices can be allocated.", FailureKind.Validation);
    public static readonly ResultError SameCustomerRequired =
        new("ar_receipt.same_customer_required", "All allocated invoices must belong to the same customer as the receipt.", FailureKind.Validation);
    public static readonly ResultError SameCurrencyRequired =
        new("ar_receipt.same_currency_required", "All allocated invoices must use the same currency as the receipt.", FailureKind.Validation);
    public static readonly ResultError AllocationTotalExceedsUnapplied =
        new("ar_receipt.allocation_total_exceeds_unapplied", "Allocation total exceeds the receipt's unapplied amount.", FailureKind.Validation);
    public static readonly ResultError InvoiceNotFound =
        new("ar_receipt.invoice_not_found", "AR invoice was not found.", FailureKind.NotFound);
    public static readonly ResultError ReceiptNotFound =
        new("ar_receipt.receipt_not_found", "AR receipt was not found.", FailureKind.NotFound);
    public static readonly ResultError OnlyDraftReceiptsAllowed =
        new("ar_receipt.only_draft_receipts_allowed", "Only draft AR receipts can be allocated.", FailureKind.Validation);
    public static readonly ResultError AllocationBaseCurrencyOnly =
        new("ar_receipt.allocation_base_currency_only", "Only base currency AR receipts can be allocated.", FailureKind.Validation);
    public static readonly ResultError UnexpectedAllocateFailure =
        new("ar_receipt.unexpected_allocate_failure", "Unexpected error while allocating AR receipt.", FailureKind.Unexpected);

    public static ResultError InvoiceWithoutRemainingBalance(string docNo) =>
        new("ar_receipt.invoice_without_remaining_balance", $"AR invoice {docNo} has no remaining balance to allocate.", FailureKind.Validation);

    public static ResultError AllocationExceedsInvoiceBalance(string docNo) =>
        new("ar_receipt.allocation_exceeds_invoice_balance", $"Allocation exceeds the remaining balance of AR invoice {docNo}.", FailureKind.Validation);
}
