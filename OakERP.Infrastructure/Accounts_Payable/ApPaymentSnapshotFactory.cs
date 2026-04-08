using OakERP.Application.AccountsPayable;
using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Accounts_Payable;

public static class ApPaymentSnapshotFactory
{
    public static ApPaymentCommandResultDto BuildSuccess(
        ApPayment payment,
        IEnumerable<ApInvoice> invoices,
        string message,
        IReadOnlyDictionary<Guid, decimal>? settledAmountOverrides = null,
        IReadOnlyCollection<ApPaymentAllocation>? allocationOverrides = null
    ) =>
        ApPaymentCommandResultDto.SuccessWith(
            BuildPaymentSnapshot(payment, allocationOverrides),
            BuildInvoiceSnapshots(invoices, settledAmountOverrides),
            message
        );

    public static ApPaymentSnapshotDto BuildPaymentSnapshot(
        ApPayment payment,
        IReadOnlyCollection<ApPaymentAllocation>? allocationOverrides = null
    )
    {
        ArgumentNullException.ThrowIfNull(payment);

        IEnumerable<ApPaymentAllocation> allocations =
            allocationOverrides ?? [.. payment.Allocations];

        return new ApPaymentSnapshotDto
        {
            PaymentId = payment.Id,
            DocNo = payment.DocNo,
            VendorId = payment.VendorId,
            BankAccountId = payment.BankAccountId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            DocStatus = payment.DocStatus,
            Memo = payment.Memo,
            AllocatedAmount = ApSettlementCalculator.GetPaymentAllocatedAmount(
                payment,
                allocations
            ),
            UnappliedAmount = ApSettlementCalculator.GetPaymentUnappliedAmount(
                payment,
                allocations
            ),
            Allocations =
            [
                .. allocations
                    .OrderBy(x => x.AllocationDate)
                    .ThenBy(x => x.Id)
                    .Select(x => new ApPaymentAllocationSnapshotDto
                    {
                        AllocationId = x.Id,
                        ApInvoiceId = x.ApInvoiceId,
                        AllocationDate = x.AllocationDate,
                        AmountApplied = x.AmountApplied,
                    }),
            ],
        };
    }

    public static IReadOnlyList<ApInvoiceSettlementSnapshotDto> BuildInvoiceSnapshots(
        IEnumerable<ApInvoice> invoices,
        IReadOnlyDictionary<Guid, decimal>? settledAmountOverrides = null
    )
    {
        ArgumentNullException.ThrowIfNull(invoices);

        return
        [
            .. invoices
                .OrderBy(x => x.DocNo, StringComparer.Ordinal)
                .Select(x =>
                {
                    decimal settledAmount =
                        settledAmountOverrides?.TryGetValue(x.Id, out decimal overrideAmount)
                        == true
                            ? overrideAmount
                            : ApSettlementCalculator.GetInvoiceSettledAmount(x);

                    return new ApInvoiceSettlementSnapshotDto
                    {
                        InvoiceId = x.Id,
                        DocNo = x.DocNo,
                        DocStatus = x.DocStatus,
                        DocTotal = x.DocTotal,
                        SettledAmount = settledAmount,
                        RemainingAmount = x.DocTotal - settledAmount,
                    };
                }),
        ];
    }
}
