using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsPayable;

public sealed class ApSettlementCalculatorTests
{
    [Fact]
    public void GetInvoiceRemainingAmount_Should_Include_Discounts_And_WriteOffs()
    {
        var invoice = new ApInvoice { DocTotal = 100m };
        invoice.Allocations.Add(
            new ApPaymentAllocation
            {
                AmountApplied = 50m,
                DiscountTaken = 10m,
                WriteOffAmount = 8m,
            }
        );

        ApSettlementCalculator.GetInvoiceRemainingAmount(invoice).ShouldBe(32m);
    }

    [Fact]
    public void GetPaymentUnappliedAmount_Should_Subtract_Existing_Allocations()
    {
        var payment = new ApPayment { Amount = 150m };
        payment.Allocations.Add(new ApPaymentAllocation { AmountApplied = 75m });

        ApSettlementCalculator.GetPaymentAllocatedAmount(payment).ShouldBe(75m);
        ApSettlementCalculator.GetPaymentUnappliedAmount(payment).ShouldBe(75m);
    }
}
