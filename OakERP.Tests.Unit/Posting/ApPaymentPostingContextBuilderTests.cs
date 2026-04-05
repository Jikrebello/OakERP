using OakERP.Infrastructure.Posting.Accounts_Payable;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ApPaymentPostingContextBuilderTests
{
    private readonly ApPaymentPostingContextBuilder _builder = new();

    [Fact]
    public async Task BuildAsync_Should_Create_Context_With_Bank_And_Settlement_Amounts()
    {
        var payment = PostingServiceTestFactory.CreateApPayment(amount: 150m, allocatedAmount: 60m);
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var settings = PostingServiceTestFactory.CreateSettings();
        var rule = PostingServiceTestFactory.CreateApPaymentRule();

        var context = await _builder.BuildAsync(
            payment,
            payment.PaymentDate,
            period,
            settings,
            rule
        );

        context.BankAccountNo.ShouldBe("1000");
        context.AllocatedAmount.ShouldBe(60m);
        context.UnappliedAmount.ShouldBe(90m);
    }

    [Fact]
    public async Task BuildAsync_Should_Reject_When_Bank_Gl_Account_Is_Missing()
    {
        var payment = PostingServiceTestFactory.CreateApPayment();
        payment.BankAccount.GlAccountNo = null!;

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _builder.BuildAsync(
                payment,
                payment.PaymentDate,
                PostingServiceTestFactory.CreateOpenPeriod(),
                PostingServiceTestFactory.CreateSettings(),
                PostingServiceTestFactory.CreateApPaymentRule()
            )
        );

        ex.Message.ShouldContain("bank account GL account");
    }
}
