using System.Net;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;

namespace OakERP.Application.AccountsPayable.Payments.Support;

internal static class ApPaymentSettlementAdapters
{
    public static SettlementInvoiceLoadSpec<ApInvoice, ApPaymentCommandResultDto> CreateInvoiceLoadSpec(
        IApInvoiceRepository apInvoiceRepository,
        Guid vendorId,
        string baseCurrencyCode
    ) =>
        new(
            (invoiceIds, cancellationToken) =>
                apInvoiceRepository.GetTrackedForSettlementAsync(invoiceIds, cancellationToken),
            invoice =>
                new SettlementInvoiceSnapshot(
                    invoice.Id,
                    invoice.DocNo,
                    invoice.DocStatus,
                    invoice.VendorId,
                    invoice.CurrencyCode,
                    ApSettlementCalculator.GetInvoiceRemainingAmount(invoice)
                ),
            new SettlementInvoiceLoadExpectations(vendorId, baseCurrencyCode),
            new SettlementInvoiceLoadFailures<ApPaymentCommandResultDto>(
                ApPaymentCommandResultDto.Fail(
                    "One or more AP invoices were not found.",
                    HttpStatusCode.NotFound
                ),
                ApPaymentCommandResultDto.Fail(
                    "Only posted AP invoices can be allocated in this slice.",
                    HttpStatusCode.BadRequest
                ),
                ApPaymentCommandResultDto.Fail(
                    "AP payment allocations must reference invoices for the same vendor.",
                    HttpStatusCode.BadRequest
                ),
                ApPaymentCommandResultDto.Fail(
                    "AP payment allocation currently supports only invoices in the base currency.",
                    HttpStatusCode.BadRequest
                ),
                docNo =>
                    ApPaymentCommandResultDto.Fail(
                        $"AP invoice {docNo} has no remaining balance to allocate.",
                        HttpStatusCode.BadRequest
                    )
            )
        );

    public static IReadOnlyList<SettlementAllocationInput> CreateAllocationInputs(
        IReadOnlyList<ApPaymentAllocationInputDto> allocations
    ) => [.. allocations.Select(input => new SettlementAllocationInput(input.ApInvoiceId, input.AmountApplied))];

    public static SettlementAllocationApplySpec<ApPaymentAllocation, ApPaymentCommandResultDto> CreateAllocationApplySpec(
        ApPayment payment,
        IReadOnlyDictionary<Guid, ApInvoice> invoices,
        IApPaymentAllocationRepository apPaymentAllocationRepository
    ) =>
        new(
            () => [.. payment.Allocations],
            () => ApSettlementCalculator.GetPaymentUnappliedAmount(payment),
            (performedBy, updatedAt) =>
            {
                payment.UpdatedAt = updatedAt;
                payment.UpdatedBy = performedBy;
            },
            invoiceId =>
            {
                if (!invoices.TryGetValue(invoiceId, out ApInvoice? invoice))
                {
                    return null;
                }

                return new SettlementAllocationInvoiceAdapter(
                    invoice.Id,
                    invoice.DocNo,
                    () => ApSettlementCalculator.GetInvoiceSettledAmount(invoice),
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
                new ApPaymentAllocation
                {
                    ApPaymentId = payment.Id,
                    ApInvoiceId = input.InvoiceId,
                    AllocationDate = allocationDate,
                    AmountApplied = input.AmountApplied,
                },
            apPaymentAllocationRepository.AddAsync,
            new SettlementAllocationFailures<ApPaymentCommandResultDto>(
                ApPaymentCommandResultDto.Fail(
                    "Allocation total exceeds the payment's unapplied amount.",
                    HttpStatusCode.BadRequest
                ),
                ApPaymentCommandResultDto.Fail(
                    "AP invoice was not found.",
                    HttpStatusCode.NotFound
                ),
                docNo =>
                    ApPaymentCommandResultDto.Fail(
                        $"Allocation amount exceeds the remaining balance for invoice {docNo}.",
                        HttpStatusCode.BadRequest
                    )
            )
        );
}
