using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Domain.Accounts_Payable;

public static class ApSettlementCalculator
{
    public static decimal GetPaymentAllocatedAmount(
        ApPayment payment,
        IEnumerable<ApPaymentAllocation>? allocations = null
    )
    {
        ArgumentNullException.ThrowIfNull(payment);

        return (allocations ?? payment.Allocations).Sum(x => x.AmountApplied);
    }

    public static decimal GetPaymentUnappliedAmount(
        ApPayment payment,
        IEnumerable<ApPaymentAllocation>? allocations = null
    )
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Amount - GetPaymentAllocatedAmount(payment, allocations);
    }

    public static decimal GetInvoiceSettledAmount(ApInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return invoice.Allocations.Sum(x =>
            x.AmountApplied + (x.DiscountTaken ?? 0m) + (x.WriteOffAmount ?? 0m)
        );
    }

    public static decimal GetInvoiceRemainingAmount(ApInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return invoice.DocTotal - GetInvoiceSettledAmount(invoice);
    }

    public static bool MatchesCurrency(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
