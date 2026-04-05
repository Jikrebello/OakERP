using OakERP.Domain.Posting;
using OakERP.Infrastructure.Posting;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingEngineApInvoiceTests
{
    [Fact]
    public void PostApInvoice_Should_Create_ApControl_Expense_And_Tax_Entries()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 15m);
        var context = PostingServiceTestFactory.CreateApInvoicePostingContext(invoice);
        var engine = new PostingEngine();

        var result = engine.PostApInvoice(context);

        result.InventoryMovements.Count.ShouldBe(0);
        result.GlEntries.Count.ShouldBe(4);
        result.GlEntries.Sum(x => x.Debit).ShouldBe(result.GlEntries.Sum(x => x.Credit));
        result.GlEntries.ShouldContain(x => x.AccountNo == "2000" && x.Credit == 115m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "5000" && x.Debit == 60m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "5100" && x.Debit == 40m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "2200" && x.Debit == 15m);
        result.GlEntries.All(x => x.SourceType == PostingSourceTypes.ApInvoice).ShouldBeTrue();
    }

    [Fact]
    public void PostApInvoice_Should_Skip_Tax_Row_When_TaxTotal_Is_Zero()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 0m);
        var context = PostingServiceTestFactory.CreateApInvoicePostingContext(invoice);
        var engine = new PostingEngine();

        var result = engine.PostApInvoice(context);

        result.InventoryMovements.Count.ShouldBe(0);
        result.GlEntries.Count.ShouldBe(3);
        result.GlEntries.ShouldContain(x => x.AccountNo == "2000" && x.Credit == 100m);
        result.GlEntries.Count(x => x.AccountNo == "2200").ShouldBe(0);
    }
}
