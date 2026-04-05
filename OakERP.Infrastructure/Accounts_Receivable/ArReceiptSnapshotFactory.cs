using OakERP.Application.AccountsReceivable;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Accounts_Receivable;

public sealed class ArReceiptSnapshotFactory
{
    public ArReceiptCommandResultDTO BuildSuccess(
        ArReceipt receipt,
        IEnumerable<ArInvoice> invoices,
        string message,
        IReadOnlyDictionary<Guid, decimal>? settledAmountOverrides = null,
        IReadOnlyCollection<ArReceiptAllocation>? allocationOverrides = null
    ) =>
        ArReceiptCommandResultDTO.SuccessWith(
            BuildReceiptSnapshot(receipt, allocationOverrides),
            BuildInvoiceSnapshots(invoices, settledAmountOverrides),
            message
        );

    public ArReceiptSnapshotDTO BuildReceiptSnapshot(
        ArReceipt receipt,
        IReadOnlyCollection<ArReceiptAllocation>? allocationOverrides = null
    )
    {
        ArgumentNullException.ThrowIfNull(receipt);

        IEnumerable<ArReceiptAllocation> allocations =
            allocationOverrides ?? [.. receipt.Allocations];

        return new ArReceiptSnapshotDTO
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
            Allocations = allocations
                .OrderBy(x => x.AllocationDate)
                .ThenBy(x => x.Id)
                .Select(x => new ArReceiptAllocationSnapshotDTO
                {
                    AllocationId = x.Id,
                    ArInvoiceId = x.ArInvoiceId,
                    AllocationDate = x.AllocationDate,
                    AmountApplied = x.AmountApplied,
                })
                .ToList(),
        };
    }

    public IReadOnlyList<ArInvoiceSettlementSnapshotDTO> BuildInvoiceSnapshots(
        IEnumerable<ArInvoice> invoices,
        IReadOnlyDictionary<Guid, decimal>? settledAmountOverrides = null
    )
    {
        ArgumentNullException.ThrowIfNull(invoices);

        return invoices
            .OrderBy(x => x.DocNo, StringComparer.Ordinal)
            .Select(x =>
            {
                decimal settledAmount =
                    settledAmountOverrides?.TryGetValue(x.Id, out decimal overrideAmount) == true
                        ? overrideAmount
                        : ArSettlementCalculator.GetInvoiceSettledAmount(x);

                return new ArInvoiceSettlementSnapshotDTO
                {
                    InvoiceId = x.Id,
                    DocNo = x.DocNo,
                    DocStatus = x.DocStatus,
                    DocTotal = x.DocTotal,
                    SettledAmount = settledAmount,
                    RemainingAmount = x.DocTotal - settledAmount,
                };
            })
            .ToList();
    }
}
