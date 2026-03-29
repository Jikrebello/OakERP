using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Infrastructure.Posting.Accounts_Receivable;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ArInvoicePostingEngineTests
{
    private readonly ArInvoicePostingEngine _engine = new();

    [Fact]
    public async Task PostArInvoice_Should_Create_Balanced_GlEntries_With_Tax()
    {
        var provider = new ArInvoicePostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateInvoice();
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var context = new ArInvoicePostingContext(
            invoice,
            invoice.Lines.OrderBy(x => x.LineNo).ToList(),
            new DateOnly(2026, 3, 15),
            period,
            settings.BaseCurrencyCode,
            1m,
            settings,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);

        result.InventoryMovements.Count.ShouldBe(0);
        result.GlEntries.Count.ShouldBe(3);
        result.GlEntries.Sum(x => x.Debit).ShouldBe(result.GlEntries.Sum(x => x.Credit));

        result.GlEntries.Single(x => x.Debit == 115m).AccountNo.ShouldBe("1100");
        result.GlEntries.Single(x => x.Credit == 100m).AccountNo.ShouldBe("4000");
        result.GlEntries.Single(x => x.Credit == 15m).AccountNo.ShouldBe("2100");
    }

    [Fact]
    public async Task PostArInvoice_Should_Omit_Tax_Line_When_TaxTotal_Is_Zero()
    {
        var provider = new ArInvoicePostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateInvoice();
        invoice.DocTotal = 100m;
        invoice.TaxTotal = 0m;

        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var context = new ArInvoicePostingContext(
            invoice,
            invoice.Lines.ToList(),
            invoice.InvoiceDate,
            period,
            settings.BaseCurrencyCode,
            1m,
            settings,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);

        result.GlEntries.Count.ShouldBe(2);
        result.GlEntries.ShouldNotContain(x => x.AccountNo == "2100");
    }

    [Fact]
    public async Task PostArInvoice_Should_Resolve_Revenue_Accounts_In_Configured_Order()
    {
        var provider = new ArInvoicePostingRuleProvider();
        var category = new ItemCategory { RevenueAccount = "4300" };
        var settings = PostingServiceTestFactory.CreateSettings() with { DefaultRevenueAccountNo = "4400" };
        var invoice = new ArInvoice
        {
            Id = Guid.NewGuid(),
            DocNo = "AR-1002",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = new DateOnly(2026, 3, 15),
            DueDate = new DateOnly(2026, 4, 14),
            CurrencyCode = "ZAR",
            TaxTotal = 0m,
            DocTotal = 40m,
            Lines =
            [
                new ArInvoiceLine
                {
                    LineNo = 1,
                    LineTotal = 10m,
                    RevenueAccount = "4100",
                    Item = new Item { Type = ItemType.Service, DefaultRevenueAccountNo = "9999", Category = category },
                },
                new ArInvoiceLine
                {
                    LineNo = 2,
                    LineTotal = 10m,
                    Item = new Item { Type = ItemType.Nonstock, DefaultRevenueAccountNo = "4200", Category = category },
                },
                new ArInvoiceLine
                {
                    LineNo = 3,
                    LineTotal = 10m,
                    Item = new Item { Type = ItemType.Service, Category = category },
                },
                new ArInvoiceLine
                {
                    LineNo = 4,
                    LineTotal = 10m,
                    Item = new Item { Type = ItemType.Nonstock },
                },
            ],
        };

        var context = new ArInvoicePostingContext(
            invoice,
            invoice.Lines.OrderBy(x => x.LineNo).ToList(),
            invoice.InvoiceDate,
            PostingServiceTestFactory.CreateOpenPeriod(),
            settings.BaseCurrencyCode,
            1m,
            settings,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);
        var revenueLines = result.GlEntries.Where(x => x.Credit > 0m).Select(x => x.AccountNo).ToList();

        revenueLines.ShouldBe(["4100", "4200", "4300", "4400"]);
    }
}
