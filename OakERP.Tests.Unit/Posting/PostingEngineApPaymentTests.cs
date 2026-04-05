using OakERP.Domain.Posting;
using OakERP.Infrastructure.Posting;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingEngineApPaymentTests
{
    [Fact]
    public void PostApPayment_Should_Create_Bank_And_ApControl_Entries_For_Full_Payment_Amount()
    {
        var payment = PostingServiceTestFactory.CreateApPayment(amount: 150m, allocatedAmount: 60m);
        var context = PostingServiceTestFactory.CreateApPaymentPostingContext(payment);
        var engine = new PostingEngine();

        var result = engine.PostApPayment(context);

        result.InventoryMovements.Count.ShouldBe(0);
        result.GlEntries.Count.ShouldBe(2);
        result.GlEntries.Sum(x => x.Debit).ShouldBe(result.GlEntries.Sum(x => x.Credit));
        result.GlEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 150m);
        result.GlEntries.ShouldContain(x => x.AccountNo == "2000" && x.Credit == 150m);
        result.GlEntries.All(x => x.SourceType == PostingSourceTypes.ApPayment).ShouldBeTrue();
    }
}
