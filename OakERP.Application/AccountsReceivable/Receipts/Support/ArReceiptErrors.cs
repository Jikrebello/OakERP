using System.Net;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

internal static class ArReceiptErrors
{
    public static readonly ResultError DocumentNumberRequired =
        new("Document number is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError DocumentNumberTooLong =
        new("Document number may not exceed 40 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError CustomerIdRequired =
        new("Customer id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError BankAccountIdRequired =
        new("Bank account id is required.", HttpStatusCode.BadRequest);

    public static readonly ResultError ReceiptAmountInvalid =
        new("Receipt amount must be greater than zero.", HttpStatusCode.BadRequest);

    public static readonly ResultError MemoTooLong =
        new("Receipt memo may not exceed 512 characters.", HttpStatusCode.BadRequest);

    public static readonly ResultError BaseCurrencyOnly =
        new(
            "AR receipt capture currently supports only the base currency.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError ReceiptIdRequired =
        new("Receipt id is required.", HttpStatusCode.BadRequest);

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

    public static readonly ResultError CustomerNotFound =
        new("Customer was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError CustomerInactive =
        new(
            "AR receipts can be created only for active customers.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError BankAccountNotFound =
        new("Bank account was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError BankAccountInactive =
        new(
            "AR receipts can be created only against active bank accounts.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError ReceiptCurrencyMismatch =
        new(
            "Bank account currency must match the receipt currency.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError DuplicateDocumentNumber =
        new(
            "An AR receipt with this document number already exists.",
            HttpStatusCode.Conflict
        );

    public static readonly ResultError AllocationConcurrencyConflict =
        new(
            "The receipt or one of its invoices was modified during allocation.",
            HttpStatusCode.Conflict
        );

    public static readonly ResultError UnexpectedCreateFailure =
        new(
            "An unexpected error occurred while creating the AR receipt.",
            HttpStatusCode.InternalServerError
        );

    public static readonly ResultError InvoicesNotFound =
        new("One or more AR invoices were not found.", HttpStatusCode.NotFound);

    public static readonly ResultError OnlyPostedInvoicesAllowed =
        new(
            "Only posted AR invoices can be allocated in this slice.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError SameCustomerRequired =
        new(
            "AR receipt allocations must reference invoices for the same customer.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError SameCurrencyRequired =
        new(
            "AR receipt allocations must reference invoices in the same currency as the receipt.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError AllocationTotalExceedsUnapplied =
        new(
            "Allocation total exceeds the receipt's unapplied amount.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError InvoiceNotFound =
        new("AR invoice was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError ReceiptNotFound =
        new("AR receipt was not found.", HttpStatusCode.NotFound);

    public static readonly ResultError OnlyDraftReceiptsAllowed =
        new(
            "Only draft AR receipts can be allocated in this slice.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError AllocationBaseCurrencyOnly =
        new(
            "AR receipt allocation currently supports only receipts in the base currency.",
            HttpStatusCode.BadRequest
        );

    public static readonly ResultError UnexpectedAllocateFailure =
        new(
            "An unexpected error occurred while allocating the AR receipt.",
            HttpStatusCode.InternalServerError
        );

    public static ResultError InvoiceWithoutRemainingBalance(string docNo) =>
        new($"AR invoice {docNo} has no remaining balance to allocate.", HttpStatusCode.BadRequest);

    public static ResultError AllocationExceedsInvoiceBalance(string docNo) =>
        new(
            $"Allocation amount exceeds the remaining balance for invoice {docNo}.",
            HttpStatusCode.BadRequest
        );
}
