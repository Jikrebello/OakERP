using OakERP.Common.Errors;

namespace OakERP.Application.AccountsReceivable.Invoices.Support;

public static class ArInvoiceErrors
{
    public static readonly ResultError DocumentNumberRequired = new(
        "ar_invoice.document_number_required",
        "Document number is required.",
        FailureKind.Validation
    );

    public static readonly ResultError DocumentNumberTooLong = new(
        "ar_invoice.document_number_too_long",
        "Document number may not exceed 40 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError CustomerIdRequired = new(
        "ar_invoice.customer_id_required",
        "Customer id is required.",
        FailureKind.Validation
    );

    public static readonly ResultError InvoiceDateRequired = new(
        "ar_invoice.invoice_date_required",
        "Invoice date is required.",
        FailureKind.Validation
    );

    public static readonly ResultError DueDateBeforeInvoiceDate = new(
        "ar_invoice.due_date_before_invoice_date",
        "Due date may not be earlier than the invoice date.",
        FailureKind.Validation
    );

    public static readonly ResultError ShipToTooLong = new(
        "ar_invoice.ship_to_too_long",
        "Ship-to value may not exceed 512 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError MemoTooLong = new(
        "ar_invoice.memo_too_long",
        "Invoice memo may not exceed 512 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError CurrencyCodeInvalid = new(
        "ar_invoice.currency_code_invalid",
        "Currency code must be a 3-character ISO code.",
        FailureKind.Validation
    );

    public static readonly ResultError TaxTotalNegative = new(
        "ar_invoice.tax_total_negative",
        "Tax total may not be negative.",
        FailureKind.Validation
    );

    public static readonly ResultError DocumentTotalNegative = new(
        "ar_invoice.document_total_negative",
        "Document total may not be negative.",
        FailureKind.Validation
    );

    public static readonly ResultError InvoiceLineRequired = new(
        "ar_invoice.invoice_line_required",
        "At least one invoice line is required.",
        FailureKind.Validation
    );

    public static readonly ResultError DocumentTotalMismatch = new(
        "ar_invoice.document_total_mismatch",
        "Document total must equal the sum of the line totals plus tax total.",
        FailureKind.Validation
    );

    public static readonly ResultError LineDescriptionTooLong = new(
        "ar_invoice.line_description_too_long",
        "Line description may not exceed 512 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError LineRevenueAccountTooLong = new(
        "ar_invoice.line_revenue_account_too_long",
        "Revenue account number may not exceed 20 characters.",
        FailureKind.Validation
    );

    public static readonly ResultError LineAmountsNegative = new(
        "ar_invoice.line_amounts_negative",
        "Line quantities and amounts may not be negative.",
        FailureKind.Validation
    );

    public static readonly ResultError LineRequiresRevenueAccountOrItem = new(
        "ar_invoice.line_requires_revenue_account_or_item",
        "Each AR invoice line must specify either a revenue account or an item.",
        FailureKind.Validation
    );

    public static readonly ResultError CustomerNotFound = new(
        "ar_invoice.customer_not_found",
        "Customer was not found.",
        FailureKind.NotFound
    );

    public static readonly ResultError CustomerInactive = new(
        "ar_invoice.customer_inactive",
        "AR invoices can be created only for active customers.",
        FailureKind.Validation
    );

    public static readonly ResultError CurrencyMissingOrInactive = new(
        "ar_invoice.currency_missing_or_inactive",
        "Currency is missing or inactive.",
        FailureKind.Validation
    );

    public static readonly ResultError DuplicateDocumentNumber = new(
        "ar_invoice.duplicate_document_number",
        "Document number already exists.",
        FailureKind.Conflict
    );

    public static readonly ResultError UnexpectedCreateFailure = new(
        "ar_invoice.unexpected_create_failure",
        "Unexpected error while creating AR invoice.",
        FailureKind.Unexpected
    );

    public static ResultError InactiveOrMissingRevenueAccount(string accountNo) =>
        new(
            "ar_invoice.revenue_account_missing_or_inactive",
            $"Revenue account '{accountNo}' is missing or inactive.",
            FailureKind.Validation
        );

    public static ResultError InactiveOrMissingItem(Guid itemId) =>
        new(
            "ar_invoice.item_missing_or_inactive",
            $"Item '{itemId}' is missing or inactive.",
            FailureKind.Validation
        );

    public static ResultError InactiveOrMissingLocation(Guid locationId) =>
        new(
            "ar_invoice.location_missing_or_inactive",
            $"Location '{locationId}' is missing or inactive.",
            FailureKind.Validation
        );

    public static ResultError InactiveOrMissingTaxRate(Guid taxRateId) =>
        new(
            "ar_invoice.tax_rate_missing_or_inactive",
            $"Tax rate '{taxRateId}' is missing or inactive.",
            FailureKind.Validation
        );

    public static ResultError InputTaxRateNotAllowed(Guid taxRateId) =>
        new(
            "ar_invoice.tax_rate_input_not_allowed",
            $"Tax rate '{taxRateId}' is marked as input tax and cannot be used on AR invoice lines.",
            FailureKind.Validation
        );
}
