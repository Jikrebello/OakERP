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
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.InvoicesNotFound),
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.OnlyPostedInvoicesAllowed),
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.SameVendorRequired),
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.BaseCurrencyInvoicesOnly),
                docNo => ApPaymentCommandResultDto.Fail(ApPaymentErrors.InvoiceWithoutRemainingBalance(docNo))
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
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationTotalExceedsUnapplied),
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.InvoiceNotFound),
                docNo =>
                    ApPaymentCommandResultDto.Fail(
                        ApPaymentErrors.AllocationExceedsInvoiceBalance(docNo)
                    )
            )
        );
}
