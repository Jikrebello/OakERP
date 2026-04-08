using OakERP.Common.Errors;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArInvoiceCommandValidatorTests
{
    [Fact]
    public void ValidateCreate_Should_Normalize_Fields_And_Allow_Mixed_Lines()
    {
        var command = new CreateArInvoiceCommand
        {
            DocNo = "  arinv-3001  ",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "  zar ",
            ShipTo = "  Warehouse 4  ",
            Memo = "  Mixed invoice  ",
            TaxTotal = 15m,
            DocTotal = 165m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "  Services  ",
                    RevenueAccount = " 4000 ",
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
                new ArInvoiceLineInputDto
                {
                    Description = "  Item line  ",
                    ItemId = Guid.NewGuid(),
                    LocationId = Guid.NewGuid(),
                    TaxRateId = Guid.NewGuid(),
                    Qty = 1m,
                    UnitPrice = 100m,
                    LineTotal = 100m,
                },
            ],
        };

        var result = ArInvoiceCommandValidator.ValidateCreate(command);

        result.Failure.ShouldBeNull();
        result.DocNo.ShouldBe("arinv-3001");
        result.CurrencyCode.ShouldBe("ZAR");
        result.ShipTo.ShouldBe("Warehouse 4");
        result.Memo.ShouldBe("Mixed invoice");
        result.PerformedBy.ShouldBe("system");
        result.Lines[0].RevenueAccount.ShouldBe("4000");
        result.Lines[1].ItemId.ShouldNotBeNull();
    }

    [Fact]
    public void ValidateCreate_Should_Reject_Line_Without_RevenueAccount_Or_Item()
    {
        var command = new CreateArInvoiceCommand
        {
            DocNo = "ARINV-3002",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DaysFromToday(-4),
            DocTotal = 10m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Invalid line",
                    Qty = 1m,
                    UnitPrice = 10m,
                    LineTotal = 10m,
                },
            ],
        };

        var result = ArInvoiceCommandValidator.ValidateCreate(command);

        result.Failure.ShouldNotBeNull();
        result.Failure!.FailureKind.ShouldBe(FailureKind.Validation);
        result.Failure.Message.ShouldContain("revenue account or an item");
    }

    [Fact]
    public void ValidateCreate_Should_Reject_Total_Mismatch()
    {
        var command = new CreateArInvoiceCommand
        {
            DocNo = "ARINV-3003",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DaysFromToday(-4),
            TaxTotal = 5m,
            DocTotal = 20m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    RevenueAccount = "4000",
                    Qty = 1m,
                    UnitPrice = 10m,
                    LineTotal = 10m,
                },
            ],
        };

        var result = ArInvoiceCommandValidator.ValidateCreate(command);

        result.Failure.ShouldNotBeNull();
        result.Failure!.FailureKind.ShouldBe(FailureKind.Validation);
        result.Failure.Message.ShouldContain("Document total");
    }
}
