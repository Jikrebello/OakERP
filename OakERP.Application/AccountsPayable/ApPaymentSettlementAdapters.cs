using System.Net;
using OakERP.Application.Settlements;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;

namespace OakERP.Application.AccountsPayable;

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
            invoice => invoice.Id,
            invoice => invoice.DocNo,
            invoice => invoice.DocStatus,
            invoice => invoice.VendorId,
            invoice => invoice.CurrencyCode,
            ApSettlementCalculator.GetInvoiceRemainingAmount,
            vendorId,
            baseCurrencyCode,
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
        );

    public static SettlementAllocationApplySpec<
        ApPayment,
        ApInvoice,
        ApPaymentAllocation,
        ApPaymentAllocationInputDto,
        ApPaymentCommandResultDto
    > CreateAllocationApplySpec(
        IApPaymentAllocationRepository apPaymentAllocationRepository
    ) =>
        new(
            payment => payment.Id,
            payment => [.. payment.Allocations],
            payment => ApSettlementCalculator.GetPaymentUnappliedAmount(payment),
            (payment, performedBy, updatedAt) =>
            {
                payment.UpdatedAt = updatedAt;
                payment.UpdatedBy = performedBy;
            },
            invoice => invoice.Id,
            invoice => invoice.DocNo,
            ApSettlementCalculator.GetInvoiceSettledAmount,
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
            input => input.ApInvoiceId,
            input => input.AmountApplied,
            (paymentId, invoiceId, allocationDate, amountApplied) =>
                new ApPaymentAllocation
                {
                    ApPaymentId = paymentId,
                    ApInvoiceId = invoiceId,
                    AllocationDate = allocationDate,
                    AmountApplied = amountApplied,
                },
            apPaymentAllocationRepository.AddAsync,
            ApPaymentCommandResultDto.Fail(
                "Allocation total exceeds the payment's unapplied amount.",
                HttpStatusCode.BadRequest
            ),
            ApPaymentCommandResultDto.Fail("AP invoice was not found.", HttpStatusCode.NotFound),
            docNo =>
                ApPaymentCommandResultDto.Fail(
                    $"Allocation amount exceeds the remaining balance for invoice {docNo}.",
                    HttpStatusCode.BadRequest
                )
        );
}
