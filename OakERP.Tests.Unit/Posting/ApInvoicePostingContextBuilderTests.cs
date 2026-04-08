using OakERP.Infrastructure.Posting.AccountsPayable;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ApInvoicePostingContextBuilderTests
{
    private readonly ApInvoicePostingContextBuilder _builder = new();

    [Fact]
    public async Task BuildAsync_Should_Order_Lines_And_Trim_Expense_Accounts()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 15m);
        invoice.Lines.Single(x => x.LineNo == 1).AccountNo = " 5000 ";
        invoice.Lines.Single(x => x.LineNo == 2).AccountNo = " 5100 ";

        var context = await _builder.BuildAsync(
            invoice,
            invoice.InvoiceDate,
            PostingServiceTestFactory.CreateOpenPeriod(),
            PostingServiceTestFactory.CreateSettings(),
            PostingServiceTestFactory.CreateApInvoiceRule()
        );

        context.Lines.Select(x => x.Line.LineNo).ToArray().ShouldBe([1, 2]);
        context.Lines.Select(x => x.ExpenseAccountNo).ToArray().ShouldBe(["5000", "5100"]);
    }

    [Fact]
    public async Task BuildAsync_Should_Reject_Item_Lines()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 0m);
        invoice.Lines.First().ItemId = Guid.NewGuid();

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            _builder.BuildAsync(
                invoice,
                invoice.InvoiceDate,
                PostingServiceTestFactory.CreateOpenPeriod(),
                PostingServiceTestFactory.CreateSettings(),
                PostingServiceTestFactory.CreateApInvoiceRule()
            )
        );

        ex.Message.ShouldContain("ItemId");
    }

    [Fact]
    public async Task BuildAsync_Should_Reject_TaxRate_Lines()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 0m);
        invoice.Lines.First().TaxRateId = Guid.NewGuid();

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            _builder.BuildAsync(
                invoice,
                invoice.InvoiceDate,
                PostingServiceTestFactory.CreateOpenPeriod(),
                PostingServiceTestFactory.CreateSettings(),
                PostingServiceTestFactory.CreateApInvoiceRule()
            )
        );

        ex.Message.ShouldContain("TaxRateId");
    }
}
