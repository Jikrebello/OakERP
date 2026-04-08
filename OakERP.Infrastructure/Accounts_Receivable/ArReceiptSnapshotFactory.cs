using OakERP.Application.AccountsReceivable;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Accounts_Receivable;

public static class ArReceiptSnapshotFactory
{
    public static ArReceiptCommandResultDto BuildSuccess(
        ArReceipt receipt,
        IEnumerable<ArInvoice> invoices,
        string message,
        IReadOnlyDictionary<Guid, decimal>? settledAmountOverrides = null,
        IReadOnlyCollection<ArReceiptAllocation>? allocationOverrides = null
    ) =>
        ArReceiptCommandResultDto.SuccessWith(
            BuildReceiptSnapshot(receipt, allocationOverrides),
            BuildInvoiceSnapshots(invoices, settledAmountOverrides),
            message
        );

    public static ArReceiptSnapshotDto BuildReceiptSnapshot(
        ArReceipt receipt,
        IReadOnlyCollection<ArReceiptAllocation>? allocationOverrides = null
    )
    {
        ArgumentNullException.ThrowIfNull(receipt);

        IEnumerable<ArReceiptAllocation> allocations =
            allocationOverrides ?? [.. receipt.Allocations];

        return new ArReceiptSnapshotDto
        {
            ReceiptId = receipt.Id,
            DocNo = receipt.DocNo,
            CustomerId = receipt.CustomerId,
            BankAccountId = receipt.BankAccountId,
            ReceiptDate = receipt.ReceiptDate,
            Amount = receipt.Amount,
            CurrencyCode = receipt.CurrencyCode,
            DocStatus = receipt.DocStatus,
            Memo = receipt.Memo,
            AllocatedAmount = ArSettlementCalculator.GetReceiptAllocatedAmount(
                receipt,
                allocations
            ),
            UnappliedAmount = ArSettlementCalculator.GetReceiptUnappliedAmount(
                receipt,
                allocations
            ),
            Allocations =
            [
                .. allocations
                    .OrderBy(x => x.AllocationDate)
                    .ThenBy(x => x.Id)
                    .Select(x => new ArReceiptAllocationSnapshotDto
                    {
                        AllocationId = x.Id,
                        ArInvoiceId = x.ArInvoiceId,
                        AllocationDate = x.AllocationDate,
                        AmountApplied = x.AmountApplied,
                    }),
            ],
        };
    }

    public static IReadOnlyList<ArInvoiceSettlementSnapshotDto> BuildInvoiceSnapshots(
        IEnumerable<ArInvoice> invoices,
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
                            : ArSettlementCalculator.GetInvoiceSettledAmount(x);

                    return new ArInvoiceSettlementSnapshotDto
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
