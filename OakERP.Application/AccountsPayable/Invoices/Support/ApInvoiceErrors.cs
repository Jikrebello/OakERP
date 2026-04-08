using System.Net;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsPayable.Invoices.Support;

internal static class ApInvoiceErrors
{
    public static readonly ResultError DocumentNumberRequired =
        new("Document number is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError DocumentNumberTooLong =
        new("Document number may not exceed 40 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError VendorIdRequired =
        new("Vendor id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError VendorInvoiceNumberRequired =
        new("Vendor invoice number is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError VendorInvoiceNumberTooLong =
        new("Vendor invoice number may not exceed 40 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError InvoiceDateRequired =
        new("Invoice date is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError DueDateBeforeInvoiceDate =
        new("Due date may not be earlier than the invoice date.", HttpStatusCode.BadRequest);

    public static readonly ResultError MemoTooLong =
        new("Invoice memo may not exceed 512 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError CurrencyCodeInvalid =
        new("Currency code must be a 3-character ISO code.", HttpStatusCode.BadRequest);

    public static readonly ResultError TaxTotalNegative =
        new("Tax total may not be negative.", HttpStatusCode.BadRequest);

    public static readonly ResultError DocumentTotalNegative =
        new("Document total may not be negative.", HttpStatusCode.BadRequest);

    public static readonly ResultError InvoiceLineRequired =
        new("At least one invoice line is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError DocumentTotalMismatch =
        new(
            "Document total must equal the sum of line totals plus tax total.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError ItemLinesDeferred =
        new("Item-based AP invoice lines are deferred in this slice.", HttpStatusCode.BadRequest);

    public static readonly ResultError TaxRatedLinesDeferred =
        new("Tax-rated AP invoice lines are deferred in this slice.", HttpStatusCode.BadRequest);

    public static readonly ResultError LineAccountRequired =
        new("Each AP invoice line must specify a GL account.", HttpStatusCode.BadRequest);

    public static readonly ResultError LineAccountTooLong =
        new("Line account number may not exceed 20 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError LineDescriptionTooLong =
        new("Line description may not exceed 512 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError LineAmountsNegative =
        new("Line quantities and amounts may not be negative.", HttpStatusCode.BadRequest);

    public static readonly ResultError VendorNotFound =
        new("Vendor was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError VendorInactive =
        new("AP invoices can be created only for active vendors.", HttpStatusCode.BadRequest);

    public static readonly ResultError CurrencyMissingOrInactive =
        new(
            "AP invoice currency was not found or is inactive.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError DuplicateDocumentNumber =
        new(
            "An AP invoice with this document number already exists.",
            HttpStatusCode.Conflict
        );

    public static readonly ResultError DuplicateVendorInvoiceNumber =
        new(
            "This vendor invoice number already exists for the selected vendor.",
            HttpStatusCode.Conflict
        );

    public static readonly ResultError UnexpectedCreateFailure =
        new(
            "An unexpected error occurred while creating the AP invoice.",
            HttpStatusCode.InternalServerError
        );

    public static ResultError InactiveOrMissingGlAccount(string accountNo) =>
        new($"GL account '{accountNo}' is missing or inactive.", HttpStatusCode.BadRequest);
}
