using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArSettlementCalculatorTests
{
    [Fact]
    public void GetInvoiceRemainingAmount_Should_Include_Discounts_And_WriteOffs()
    {
        var invoice = new ArInvoice { DocTotal = 100m };
        invoice.Allocations.Add(
            new ArReceiptAllocation
            {
                AmountApplied = 60m,
                DiscountGiven = 5m,
                WriteOffAmount = 3m,
            }
        );

        ArSettlementCalculator.GetInvoiceSettledAmount(invoice).ShouldBe(68m);
        ArSettlementCalculator.GetInvoiceRemainingAmount(invoice).ShouldBe(32m);
    }

    [Fact]
    public void GetReceiptUnappliedAmount_Should_Subtract_Existing_Allocations()
    {
        var receipt = new ArReceipt { Amount = 150m };
        receipt.Allocations.Add(new ArReceiptAllocation { AmountApplied = 40m });
        receipt.Allocations.Add(new ArReceiptAllocation { AmountApplied = 35m });

        ArSettlementCalculator.GetReceiptAllocatedAmount(receipt).ShouldBe(75m);
        ArSettlementCalculator.GetReceiptUnappliedAmount(receipt).ShouldBe(75m);
    }

    [Fact]
    public void MatchesCurrency_Should_Ignore_Case()
    {
        ArSettlementCalculator.MatchesCurrency("zar", "ZAR").ShouldBeTrue();
        ArSettlementCalculator.MatchesCurrency("USD", "ZAR").ShouldBeFalse();
    }
}
