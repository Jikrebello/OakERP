using Moq;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting.Inventory;
using OakERP.Infrastructure.Posting.Accounts_Receivable;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ArInvoicePostingContextBuilderTests
{
    private readonly Mock<IInventoryCostService> _inventoryCostService = new(MockBehavior.Strict);

    [Fact]
    public async Task BuildAsync_Should_Reject_Stock_Line_Without_Location()
    {
        var invoice = PostingServiceTestFactory.CreateStockInvoice(includeLocation: false);
        var builder = new ArInvoicePostingContextBuilder(_inventoryCostService.Object);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            builder.BuildAsync(
                invoice,
                invoice.InvoiceDate,
                PostingServiceTestFactory.CreateOpenPeriod(),
                PostingServiceTestFactory.CreateSettings(),
                PostingServiceTestFactory.CreateRule()
            )
        );

        ex.Message.ShouldContain("requires a location");
    }

    [Fact]
    public async Task BuildAsync_Should_Reject_When_No_Prior_Cost_Basis_Exists()
    {
        var invoice = PostingServiceTestFactory.CreateStockInvoice();
        var stockLine = invoice.Lines.Single();

        _inventoryCostService
            .Setup(x =>
                x.GetUnitCostForSaleAsync(
                    stockLine.ItemId!.Value,
                    stockLine.LocationId!.Value,
                    invoice.InvoiceDate,
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("No prior cost basis exists."));

        var builder = new ArInvoicePostingContextBuilder(_inventoryCostService.Object);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            builder.BuildAsync(
                invoice,
                invoice.InvoiceDate,
                PostingServiceTestFactory.CreateOpenPeriod(),
                PostingServiceTestFactory.CreateSettings(),
                PostingServiceTestFactory.CreateRule()
            )
        );

        ex.Message.ShouldContain("prior cost basis");
    }

    [Fact]
    public async Task BuildAsync_Should_Resolve_Revenue_Accounts_In_Configured_Order()
    {
        var invoice = PostingServiceTestFactory.CreateInvoice();
        invoice.DocTotal = 40m;
        invoice.TaxTotal = 0m;
        invoice.Lines =
        [
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                Id = Guid.NewGuid(),
                LineNo = 1,
                LineTotal = 10m,
                RevenueAccount = "4100",
                Item = new Item
                {
                    Type = ItemType.Service,
                    DefaultRevenueAccountNo = "4199",
                    Category = new ItemCategory { RevenueAccount = "4198" },
                },
            },
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                Id = Guid.NewGuid(),
                LineNo = 2,
                LineTotal = 10m,
                Item = new Item
                {
                    Type = ItemType.Service,
                    DefaultRevenueAccountNo = "4200",
                    Category = new ItemCategory { RevenueAccount = "4298" },
                },
            },
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                Id = Guid.NewGuid(),
                LineNo = 3,
                LineTotal = 10m,
                Item = new Item
                {
                    Type = ItemType.Service,
                    Category = new ItemCategory { RevenueAccount = "4300" },
                },
            },
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                Id = Guid.NewGuid(),
                LineNo = 4,
                LineTotal = 10m,
            },
        ];

        var builder = new ArInvoicePostingContextBuilder(_inventoryCostService.Object);

        var context = await builder.BuildAsync(
            invoice,
            invoice.InvoiceDate,
            PostingServiceTestFactory.CreateOpenPeriod(),
            PostingServiceTestFactory.CreateSettings(),
            PostingServiceTestFactory.CreateRule()
        );

        context.Lines.Select(x => x.RevenueAccountNo).ShouldBe(["4100", "4200", "4300", "4000"]);
    }

    [Fact]
    public async Task BuildAsync_Should_Resolve_Cogs_And_Inventory_Accounts_In_Configured_Order()
    {
        var firstInvoice = PostingServiceTestFactory.CreateStockInvoice();
        var secondInvoice = PostingServiceTestFactory.CreateStockInvoice();
        secondInvoice.Lines.Single().LineNo = 2;
        secondInvoice.Lines.Single().Item!.Category = new ItemCategory();
        firstInvoice.Lines.Add(secondInvoice.Lines.Single());
        firstInvoice.DocTotal = 215m;

        foreach (var line in firstInvoice.Lines)
        {
            _inventoryCostService
                .Setup(x =>
                    x.GetUnitCostForSaleAsync(
                        line.ItemId!.Value,
                        line.LocationId!.Value,
                        firstInvoice.InvoiceDate,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(10m);
        }

        var settings = PostingServiceTestFactory.CreateSettings() with
        {
            DefaultCogsAccountNo = "5200",
            DefaultInventoryAssetAccountNo = "1310",
        };

        var builder = new ArInvoicePostingContextBuilder(_inventoryCostService.Object);

        var context = await builder.BuildAsync(
            firstInvoice,
            firstInvoice.InvoiceDate,
            PostingServiceTestFactory.CreateOpenPeriod(),
            settings,
            PostingServiceTestFactory.CreateRule()
        );

        context.Lines.Single(x => x.Line.LineNo == 1).CogsAccountNo.ShouldBe("5100");
        context.Lines.Single(x => x.Line.LineNo == 1).InventoryAssetAccountNo.ShouldBe("1300");
        context.Lines.Single(x => x.Line.LineNo == 2).CogsAccountNo.ShouldBe("5200");
        context.Lines.Single(x => x.Line.LineNo == 2).InventoryAssetAccountNo.ShouldBe("1310");
    }
}
