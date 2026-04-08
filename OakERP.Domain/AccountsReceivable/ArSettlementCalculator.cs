using OakERP.Domain.Entities.AccountsReceivable;

namespace OakERP.Domain.AccountsReceivable;

public static class ArSettlementCalculator
{
    public static decimal GetReceiptAllocatedAmount(
        ArReceipt receipt,
        IEnumerable<ArReceiptAllocation>? allocations = null
    )
    {
        ArgumentNullException.ThrowIfNull(receipt);

        return (allocations ?? receipt.Allocations).Sum(x => x.AmountApplied);
    }

    public static decimal GetReceiptUnappliedAmount(
        ArReceipt receipt,
        IEnumerable<ArReceiptAllocation>? allocations = null
    )
    {
        ArgumentNullException.ThrowIfNull(receipt);

        return receipt.Amount - GetReceiptAllocatedAmount(receipt, allocations);
    }

    public static decimal GetInvoiceSettledAmount(ArInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return invoice.Allocations.Sum(x =>
            x.AmountApplied + (x.DiscountGiven ?? 0m) + (x.WriteOffAmount ?? 0m)
        );
    }

    public static decimal GetInvoiceRemainingAmount(ArInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return invoice.DocTotal - GetInvoiceSettledAmount(invoice);
    }

    public static bool MatchesCurrency(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
