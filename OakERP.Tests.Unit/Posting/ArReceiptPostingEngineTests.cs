using OakERP.Common.Enums;
using OakERP.Infrastructure.Posting.Accounts_Receivable;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ArReceiptPostingEngineTests
{
    private readonly ArInvoicePostingEngine _engine = new();

    [Fact]
    public async Task PostArReceipt_Should_Create_Balanced_Bank_And_Ar_Control_GlEntries()
    {
        var provider = new ArInvoicePostingRuleProvider();
        var receipt = PostingServiceTestFactory.CreateReceipt(amount: 150m, allocatedAmount: 100m);
        var context = PostingServiceTestFactory.CreateReceiptPostingContext(
            receipt,
            await provider.GetActiveRuleAsync(DocKind.ArReceipt)
        );

        var result = _engine.PostArReceipt(context);

        result.InventoryMovements.ShouldBeEmpty();
        result.GlEntries.Count.ShouldBe(2);
        result.GlEntries.Sum(x => x.Debit).ShouldBe(result.GlEntries.Sum(x => x.Credit));
        result.GlEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 150m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "1100" && x.Credit == 150m);
        result
            .GlEntries.All(x => x.SourceType == OakERP.Domain.Posting.PostingSourceTypes.ArReceipt)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task PostArReceipt_Should_Use_Full_Receipt_Amount_For_Unapplied_Cash()
    {
        var provider = new ArInvoicePostingRuleProvider();
        var receipt = PostingServiceTestFactory.CreateReceipt(amount: 125m, allocatedAmount: 0m);
        var context = PostingServiceTestFactory.CreateReceiptPostingContext(
            receipt,
            await provider.GetActiveRuleAsync(DocKind.ArReceipt)
        );

        var result = _engine.PostArReceipt(context);

        result.GlEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 125m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "1100" && x.Credit == 125m);
    }
}
