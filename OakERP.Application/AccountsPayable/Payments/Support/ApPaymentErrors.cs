using OakERP.Common.Errors;
using OakERP.Application.Settlements.Documents;

namespace OakERP.Application.AccountsPayable.Payments.Support;

public static class ApPaymentErrors
{
    public static readonly ResultError DocumentNumberRequired =
        new("ap_payment.document_number_required", "Document number is required.", FailureKind.Validation);
    public static readonly ResultError DocumentNumberTooLong =
        new("ap_payment.document_number_too_long", "Document number may not exceed 40 characters.", FailureKind.Validation);
    public static readonly ResultError VendorIdRequired =
        new("ap_payment.vendor_id_required", "Vendor id is required.", FailureKind.Validation);
    public static readonly ResultError BankAccountIdRequired =
        new("ap_payment.bank_account_id_required", "Bank account id is required.", FailureKind.Validation);
    public static readonly ResultError PaymentDateRequired =
        new("ap_payment.payment_date_required", "Payment date is required.", FailureKind.Validation);
    public static readonly ResultError PaymentAmountInvalid =
        new("ap_payment.payment_amount_invalid", "Payment amount must be greater than zero.", FailureKind.Validation);
    public static readonly ResultError MemoTooLong =
        new("ap_payment.memo_too_long", "Payment memo may not exceed 512 characters.", FailureKind.Validation);
    public static readonly ResultError PaymentIdRequired =
        new("ap_payment.payment_id_required", "Payment id is required.", FailureKind.Validation);
    public static readonly ResultError AllocationRequired =
        new("ap_payment.allocation_required", SettlementDocumentErrorMessages.AllocationRequired, FailureKind.Validation);
    public static readonly ResultError AllocationInvoiceIdRequired =
        new("ap_payment.allocation_invoice_id_required", "Allocation invoice id is required.", FailureKind.Validation);
    public static readonly ResultError AllocationDuplicateInvoice =
        new("ap_payment.allocation_duplicate_invoice", "Each invoice may be allocated only once per request.", FailureKind.Validation);
    public static readonly ResultError AllocationAmountInvalid =
        new("ap_payment.allocation_amount_invalid", "Allocation amount must be greater than zero.", FailureKind.Validation);
    public static readonly ResultError VendorNotFound =
        new("ap_payment.vendor_not_found", "Vendor was not found.", FailureKind.NotFound);
    public static readonly ResultError VendorInactive =
        new("ap_payment.vendor_inactive", "AP payments can be created only for active vendors.", FailureKind.Validation);
    public static readonly ResultError BankAccountNotFound =
        new("ap_payment.bank_account_not_found", SettlementDocumentErrorMessages.BankAccountNotFound, FailureKind.NotFound);
    public static readonly ResultError BankAccountInactive =
        new("ap_payment.bank_account_inactive", "AP payments can be created only for active bank accounts.", FailureKind.Validation);
    public static readonly ResultError BaseCurrencyOnly =
        new("ap_payment.base_currency_only", "AP payments currently support only bank accounts in the base currency.", FailureKind.Validation);
    public static readonly ResultError DuplicateDocumentNumber =
        new("ap_payment.duplicate_document_number", SettlementDocumentErrorMessages.DuplicateDocumentNumber, FailureKind.Conflict);
    public static readonly ResultError AllocationConcurrencyConflict =
        new("ap_payment.allocation_concurrency_conflict", "The AP payment was modified while allocations were being saved.", FailureKind.Conflict);
    public static readonly ResultError UnexpectedCreateFailure =
        new("ap_payment.unexpected_create_failure", "Unexpected error while creating AP payment.", FailureKind.Unexpected);
    public static readonly ResultError InvoicesNotFound =
        new("ap_payment.invoices_not_found", "One or more AP invoices were not found.", FailureKind.NotFound);
    public static readonly ResultError OnlyPostedInvoicesAllowed =
        new("ap_payment.only_posted_invoices_allowed", "Only posted AP invoices can be allocated.", FailureKind.Validation);
    public static readonly ResultError SameVendorRequired =
        new("ap_payment.same_vendor_required", "All allocated invoices must belong to the same vendor as the payment.", FailureKind.Validation);
    public static readonly ResultError BaseCurrencyInvoicesOnly =
        new("ap_payment.base_currency_invoices_only", "Only base currency AP invoices can be allocated.", FailureKind.Validation);
    public static readonly ResultError AllocationTotalExceedsUnapplied =
        new("ap_payment.allocation_total_exceeds_unapplied", "Allocation total exceeds the payment's unapplied amount.", FailureKind.Validation);
    public static readonly ResultError InvoiceNotFound =
        new("ap_payment.invoice_not_found", "AP invoice was not found.", FailureKind.NotFound);
    public static readonly ResultError PaymentNotFound =
        new("ap_payment.payment_not_found", "AP payment was not found.", FailureKind.NotFound);
    public static readonly ResultError OnlyDraftPaymentsAllowed =
        new("ap_payment.only_draft_payments_allowed", "Only draft AP payments can be allocated.", FailureKind.Validation);
    public static readonly ResultError AllocationBaseCurrencyOnly =
        new("ap_payment.allocation_base_currency_only", "Only base currency AP payments can be allocated.", FailureKind.Validation);
    public static readonly ResultError UnexpectedAllocateFailure =
        new("ap_payment.unexpected_allocate_failure", "Unexpected error while allocating AP payment.", FailureKind.Unexpected);

    public static ResultError InvoiceWithoutRemainingBalance(string docNo) =>
        new("ap_payment.invoice_without_remaining_balance", $"AP invoice {docNo} has no remaining balance to allocate.", FailureKind.Validation);

    public static ResultError AllocationExceedsInvoiceBalance(string docNo) =>
        new("ap_payment.allocation_exceeds_invoice_balance", $"Allocation exceeds the remaining balance of AP invoice {docNo}.", FailureKind.Validation);
}
