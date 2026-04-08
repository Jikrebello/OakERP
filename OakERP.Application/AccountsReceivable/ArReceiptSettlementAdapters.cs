using System.Net;
using OakERP.Application.Settlements;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;

namespace OakERP.Application.AccountsReceivable;

internal static class ArReceiptSettlementAdapters
{
    public static SettlementInvoiceLoadSpec<ArInvoice, ArReceiptCommandResultDto> CreateInvoiceLoadSpec(
        IArInvoiceRepository arInvoiceRepository,
        Guid customerId,
        string currencyCode
    ) =>
        new(
            (invoiceIds, cancellationToken) =>
                arInvoiceRepository.GetTrackedForAllocationAsync(invoiceIds, cancellationToken),
            invoice => invoice.Id,
            invoice => invoice.DocNo,
            invoice => invoice.DocStatus,
            invoice => invoice.CustomerId,
            invoice => invoice.CurrencyCode,
            ArSettlementCalculator.GetInvoiceRemainingAmount,
            customerId,
            currencyCode,
            ArReceiptCommandResultDto.Fail(
                "One or more AR invoices were not found.",
                HttpStatusCode.NotFound
            ),
            ArReceiptCommandResultDto.Fail(
                "Only posted AR invoices can be allocated in this slice.",
                HttpStatusCode.BadRequest
            ),
            ArReceiptCommandResultDto.Fail(
                "AR receipt allocations must reference invoices for the same customer.",
                HttpStatusCode.BadRequest
            ),
            ArReceiptCommandResultDto.Fail(
                "AR receipt allocations must reference invoices in the same currency as the receipt.",
                HttpStatusCode.BadRequest
            ),
            docNo =>
                ArReceiptCommandResultDto.Fail(
                    $"AR invoice {docNo} has no remaining balance to allocate.",
                    HttpStatusCode.BadRequest
                )
        );

    public static SettlementAllocationApplySpec<
        ArReceipt,
        ArInvoice,
        ArReceiptAllocation,
        ArReceiptAllocationInputDto,
        ArReceiptCommandResultDto
    > CreateAllocationApplySpec(
        IArReceiptAllocationRepository arReceiptAllocationRepository
    ) =>
        new(
            receipt => receipt.Id,
            receipt => [.. receipt.Allocations],
            receipt => ArSettlementCalculator.GetReceiptUnappliedAmount(receipt),
            (receipt, performedBy, updatedAt) =>
            {
                receipt.UpdatedAt = updatedAt;
                receipt.UpdatedBy = performedBy;
            },
            invoice => invoice.Id,
            invoice => invoice.DocNo,
            ArSettlementCalculator.GetInvoiceSettledAmount,
            (invoice, currentSettledAmount) => invoice.DocTotal - currentSettledAmount,
            (invoice, performedBy, updatedAt) =>
            {
                invoice.UpdatedAt = updatedAt;
                invoice.UpdatedBy = performedBy;
            },
            (invoice, remainingAfterAllocation) =>
            {
                if (remainingAfterAllocation == 0m)
                {
                    invoice.DocStatus = DocStatus.Closed;
                }
            },
            input => input.ArInvoiceId,
            input => input.AmountApplied,
            (receiptId, invoiceId, allocationDate, amountApplied) =>
                new ArReceiptAllocation
                {
                    ArReceiptId = receiptId,
                    ArInvoiceId = invoiceId,
                    AllocationDate = allocationDate,
                    AmountApplied = amountApplied,
                },
            arReceiptAllocationRepository.AddAsync,
            ArReceiptCommandResultDto.Fail(
                "Allocation total exceeds the receipt's unapplied amount.",
                HttpStatusCode.BadRequest
            ),
            ArReceiptCommandResultDto.Fail("AR invoice was not found.", HttpStatusCode.NotFound),
            docNo =>
                ArReceiptCommandResultDto.Fail(
                    $"Allocation amount exceeds the remaining balance for invoice {docNo}.",
                    HttpStatusCode.BadRequest
                )
        );
}
