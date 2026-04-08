using System.Net;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsPayable.Payments.Support;

internal static class ApPaymentErrors
{
    public static readonly ResultError DocumentNumberRequired =
        new("Document number is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError DocumentNumberTooLong =
        new("Document number may not exceed 40 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError VendorIdRequired =
        new("Vendor id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError BankAccountIdRequired =
        new("Bank account id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError PaymentDateRequired =
        new("Payment date is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError PaymentAmountInvalid =
        new("Payment amount must be greater than zero.", HttpStatusCode.BadRequest);

    public static readonly ResultError MemoTooLong =
        new("Payment memo may not exceed 512 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError PaymentIdRequired =
        new("Payment id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError AllocationRequired =
        new("At least one allocation is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError AllocationInvoiceIdRequired =
        new("Allocation invoice id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError AllocationDuplicateInvoice =
        new(
            "Each invoice may appear only once per allocation request.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError AllocationAmountInvalid =
        new("Allocation amount must be greater than zero.", HttpStatusCode.BadRequest);

    public static readonly ResultError VendorNotFound =
        new("Vendor was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError VendorInactive =
        new(
            "AP payments can be created only for active vendors.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError BankAccountNotFound =
        new("Bank account was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError BankAccountInactive =
        new(
            "AP payments can be created only against active bank accounts.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError BaseCurrencyOnly =
        new(
            "AP payment capture currently supports only the base currency.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError DuplicateDocumentNumber =
        new(
            "An AP payment with this document number already exists.",
            HttpStatusCode.Conflict
        );

    public static readonly ResultError AllocationConcurrencyConflict =
        new(
            "The payment or one of its invoices was modified during allocation.",
            HttpStatusCode.Conflict
        );

    public static readonly ResultError UnexpectedCreateFailure =
        new(
            "An unexpected error occurred while creating the AP payment.",
            HttpStatusCode.InternalServerError
        );

    public static readonly ResultError InvoicesNotFound =
        new("One or more AP invoices were not found.", HttpStatusCode.NotFound);

    public static readonly ResultError OnlyPostedInvoicesAllowed =
        new(
            "Only posted AP invoices can be allocated in this slice.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError SameVendorRequired =
        new(
            "AP payment allocations must reference invoices for the same vendor.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError BaseCurrencyInvoicesOnly =
        new(
            "AP payment allocation currently supports only invoices in the base currency.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError AllocationTotalExceedsUnapplied =
        new(
            "Allocation total exceeds the payment's unapplied amount.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError InvoiceNotFound =
        new("AP invoice was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError PaymentNotFound =
        new("AP payment was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError OnlyDraftPaymentsAllowed =
        new(
            "Only draft AP payments can be allocated in this slice.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError AllocationBaseCurrencyOnly =
        new(
            "AP payment allocation currently supports only payments in the base currency.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError UnexpectedAllocateFailure =
        new(
            "An unexpected error occurred while allocating the AP payment.",
            HttpStatusCode.InternalServerError
        );

    public static ResultError InvoiceWithoutRemainingBalance(string docNo) =>
        new($"AP invoice {docNo} has no remaining balance to allocate.", HttpStatusCode.BadRequest);

    public static ResultError AllocationExceedsInvoiceBalance(string docNo) =>
        new(
            $"Allocation amount exceeds the remaining balance for invoice {docNo}.",
            HttpStatusCode.BadRequest
        );
}
