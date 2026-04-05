using OakERP.Common.Enums;
using OakERP.Infrastructure.Posting;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingEngineArInvoiceTests
{
    private readonly PostingEngine _engine = new();

    [Fact]
    public async Task PostArInvoice_Should_Create_Balanced_GlEntries_With_Tax()
    {
        var provider = new PostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateInvoice();
        var context = PostingServiceTestFactory.CreatePostingContext(
            invoice,
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
        var provider = new PostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateInvoice();
        invoice.DocTotal = 100m;
        invoice.TaxTotal = 0m;
        var context = PostingServiceTestFactory.CreatePostingContext(
            invoice,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);

        result.GlEntries.Count.ShouldBe(2);
        result.GlEntries.ShouldNotContain(x => x.AccountNo == "2100");
    }

    [Fact]
    public async Task PostArInvoice_Should_Use_Prepared_Revenue_Accounts_From_Context()
    {
        var provider = new PostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateInvoice();
        invoice.DocTotal = 40m;
        invoice.TaxTotal = 0m;
        invoice.Lines =
        [
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                LineNo = 1,
                LineTotal = 10m,
                RevenueAccount = "4100",
            },
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                LineNo = 2,
                LineTotal = 10m,
                RevenueAccount = "4200",
            },
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                LineNo = 3,
                LineTotal = 10m,
                RevenueAccount = "4300",
            },
            new Domain.Entities.Accounts_Receivable.ArInvoiceLine
            {
                LineNo = 4,
                LineTotal = 10m,
                RevenueAccount = "4400",
            },
        ];

        var context = PostingServiceTestFactory.CreatePostingContext(
            invoice,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);
        var revenueLines = result
            .GlEntries.Where(x => x.Credit > 0m)
            .Select(x => x.AccountNo)
            .ToList();

        revenueLines.ShouldBe(["4100", "4200", "4300", "4400"]);
    }

    [Fact]
    public async Task PostArInvoice_Should_Create_Cogs_And_Inventory_Movements_For_Stock_Lines()
    {
        var provider = new PostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateStockInvoice();
        var context = PostingServiceTestFactory.CreatePostingContext(
            invoice,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);

        result.GlEntries.Count.ShouldBe(5);
        result.InventoryMovements.Count.ShouldBe(1);
        result.GlEntries.ShouldContain(x => x.AccountNo == "5100" && x.Debit == 12.35m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "1300" && x.Credit == 12.35m);

        var movement = result.InventoryMovements.Single();
        movement.TransactionType.ShouldBe(InventoryTransactionType.SalesCogs);
        movement.Qty.ShouldBe(-1m);
        movement.UnitCost.ShouldBe(12.3456m);
        movement.ValueChange.ShouldBe(-12.35m);
    }

    [Fact]
    public async Task PostArInvoice_Should_Keep_NonStock_Behavior_On_Mixed_Invoice()
    {
        var provider = new PostingRuleProvider();
        var invoice = PostingServiceTestFactory.CreateInvoice();
        var stockInvoice = PostingServiceTestFactory.CreateStockInvoice();
        stockInvoice.Lines.Single().LineNo = 2;
        stockInvoice.Lines.Single().LineTotal = 50m;
        stockInvoice.Lines.Single().Qty = 2m;
        invoice.Lines.Add(stockInvoice.Lines.Single());
        invoice.DocTotal = 165m;
        invoice.TaxTotal = 15m;

        var context = PostingServiceTestFactory.CreatePostingContext(
            invoice,
            await provider.GetActiveRuleAsync(DocKind.ArInvoice)
        );

        var result = _engine.PostArInvoice(context);

        result.GlEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 100m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 50m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "5100" && x.Debit == 24.69m);
        result.InventoryMovements.Count.ShouldBe(1);
        result.InventoryMovements.Single().Qty.ShouldBe(-2m);
        result.InventoryMovements.Single().ValueChange.ShouldBe(-24.69m);
    }
}
