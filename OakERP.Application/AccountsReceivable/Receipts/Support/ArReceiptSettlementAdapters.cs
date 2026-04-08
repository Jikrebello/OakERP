using System.Net;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsReceivable;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

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
            invoice =>
                new SettlementInvoiceSnapshot(
                    invoice.Id,
                    invoice.DocNo,
                    invoice.DocStatus,
                    invoice.CustomerId,
                    invoice.CurrencyCode,
                    ArSettlementCalculator.GetInvoiceRemainingAmount(invoice)
                ),
            new SettlementInvoiceLoadExpectations(customerId, currencyCode),
            new SettlementInvoiceLoadFailures<ArReceiptCommandResultDto>(
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
            )
        );

    public static IReadOnlyList<SettlementAllocationInput> CreateAllocationInputs(
        IReadOnlyList<ArReceiptAllocationInputDto> allocations
    ) => [.. allocations.Select(input => new SettlementAllocationInput(input.ArInvoiceId, input.AmountApplied))];

    public static SettlementAllocationApplySpec<ArReceiptAllocation, ArReceiptCommandResultDto> CreateAllocationApplySpec(
        ArReceipt receipt,
        IReadOnlyDictionary<Guid, ArInvoice> invoices,
        IArReceiptAllocationRepository arReceiptAllocationRepository
    ) =>
        new(
            () => [.. receipt.Allocations],
            () => ArSettlementCalculator.GetReceiptUnappliedAmount(receipt),
            (performedBy, updatedAt) =>
            {
                receipt.UpdatedAt = updatedAt;
                receipt.UpdatedBy = performedBy;
            },
            invoiceId =>
            {
                if (!invoices.TryGetValue(invoiceId, out ArInvoice? invoice))
                {
                    return null;
                }

                return new SettlementAllocationInvoiceAdapter(
                    invoice.Id,
                    invoice.DocNo,
                    () => ArSettlementCalculator.GetInvoiceSettledAmount(invoice),
                    currentSettledAmount => invoice.DocTotal - currentSettledAmount,
                    (performedBy, updatedAt) =>
                    {
                        invoice.UpdatedAt = updatedAt;
                        invoice.UpdatedBy = performedBy;
                    },
                    remainingAfterAllocation =>
                    {
                        if (remainingAfterAllocation == 0m)
                        {
                            invoice.DocStatus = DocStatus.Closed;
                        }
                    }
                );
            },
            (input, allocationDate) =>
                new ArReceiptAllocation
                {
                    ArReceiptId = receipt.Id,
                    ArInvoiceId = input.InvoiceId,
                    AllocationDate = allocationDate,
                    AmountApplied = input.AmountApplied,
                },
            arReceiptAllocationRepository.AddAsync,
            new SettlementAllocationFailures<ArReceiptCommandResultDto>(
                ArReceiptCommandResultDto.Fail(
                    "Allocation total exceeds the receipt's unapplied amount.",
                    HttpStatusCode.BadRequest
                ),
                ArReceiptCommandResultDto.Fail(
                    "AR invoice was not found.",
                    HttpStatusCode.NotFound
                ),
                docNo =>
                    ArReceiptCommandResultDto.Fail(
                        $"Allocation amount exceeds the remaining balance for invoice {docNo}.",
                        HttpStatusCode.BadRequest
                    )
            )
        );
}
