using OakERP.Common.Errors;

namespace OakERP.Application.AccountsPayable.Invoices.Support;

public static class ApInvoiceErrors
{
    public static readonly ResultError DocumentNumberRequired = new(
        "ap_invoice.document_number_required",
        "Document number is required.",
        FailureKind.Validation
    );

    public static readonly ResultError DocumentNumberTooLong = new(
        "ap_invoice.document_number_too_long",
        "Document number may not exceed 40 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError VendorIdRequired = new(
        "ap_invoice.vendor_id_required",
        "Vendor id is required.",
        FailureKind.Validation
    );

    public static readonly ResultError VendorInvoiceNumberRequired = new(
        "ap_invoice.vendor_invoice_number_required",
        "Vendor invoice number is required.",
        FailureKind.Validation
    );

    public static readonly ResultError VendorInvoiceNumberTooLong = new(
        "ap_invoice.vendor_invoice_number_too_long",
        "Vendor invoice number may not exceed 40 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError InvoiceDateRequired = new(
        "ap_invoice.invoice_date_required",
        "Invoice date is required.",
        FailureKind.Validation
    );

    public static readonly ResultError DueDateBeforeInvoiceDate = new(
        "ap_invoice.due_date_before_invoice_date",
        "Due date may not be earlier than the invoice date.",
        FailureKind.Validation
    );

    public static readonly ResultError MemoTooLong = new(
        "ap_invoice.memo_too_long",
        "Invoice memo may not exceed 512 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError CurrencyCodeInvalid = new(
        "ap_invoice.currency_code_invalid",
        "Currency code must be a 3-character ISO code.",
        FailureKind.Validation
    );

    public static readonly ResultError TaxTotalNegative = new(
        "ap_invoice.tax_total_negative",
        "Tax total may not be negative.",
        FailureKind.Validation
    );

    public static readonly ResultError DocumentTotalNegative = new(
        "ap_invoice.document_total_negative",
        "Document total may not be negative.",
        FailureKind.Validation
    );

    public static readonly ResultError InvoiceLineRequired = new(
        "ap_invoice.invoice_line_required",
        "At least one invoice line is required.",
        FailureKind.Validation
    );

    public static readonly ResultError DocumentTotalMismatch = new(
        "ap_invoice.document_total_mismatch",
        "Document total must equal the sum of the line totals plus tax total.",
        FailureKind.Validation
    );

    public static readonly ResultError ItemLinesDeferred = new(
        "ap_invoice.item_lines_deferred",
        "Item-based AP invoice lines are deferred in this slice.",
        FailureKind.Validation
    );

    public static readonly ResultError TaxRatedLinesDeferred = new(
        "ap_invoice.tax_rated_lines_deferred",
        "Tax-rated AP invoice lines are deferred in this slice.",
        FailureKind.Validation
    );

    public static readonly ResultError LineAccountRequired = new(
        "ap_invoice.line_account_required",
        "Each AP invoice line must specify a GL account.",
        FailureKind.Validation
    );

    public static readonly ResultError LineAccountTooLong = new(
        "ap_invoice.line_account_too_long",
        "Line account number may not exceed 20 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError LineDescriptionTooLong = new(
        "ap_invoice.line_description_too_long",
        "Line description may not exceed 512 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError LineAmountsNegative = new(
        "ap_invoice.line_amounts_negative",
        "Line quantities and amounts may not be negative.",
        FailureKind.Validation
    );

    public static readonly ResultError VendorNotFound = new(
        "ap_invoice.vendor_not_found",
        "Vendor was not found.",
        FailureKind.NotFound
    );

    public static readonly ResultError VendorInactive = new(
        "ap_invoice.vendor_inactive",
        "AP invoices can be created only for active vendors.",
        FailureKind.Validation
    );

    public static readonly ResultError CurrencyMissingOrInactive = new(
        "ap_invoice.currency_missing_or_inactive",
        "Currency is missing or inactive.",
        FailureKind.Validation
    );

    public static readonly ResultError DuplicateDocumentNumber = new(
        "ap_invoice.duplicate_document_number",
        "Document number already exists.",
        FailureKind.Conflict
    );

    public static readonly ResultError DuplicateVendorInvoiceNumber = new(
        "ap_invoice.duplicate_vendor_invoice_number",
        "Vendor invoice number already exists for this vendor.",
        FailureKind.Conflict
    );

    public static readonly ResultError UnexpectedCreateFailure = new(
        "ap_invoice.unexpected_create_failure",
        "Unexpected error while creating AP invoice.",
        FailureKind.Unexpected
    );

    public static ResultError InactiveOrMissingGlAccount(string accountNo) =>
        new(
            "ap_invoice.gl_account_missing_or_inactive",
            $"GL account '{accountNo}' is missing or inactive.",
            FailureKind.Validation
        );
}
