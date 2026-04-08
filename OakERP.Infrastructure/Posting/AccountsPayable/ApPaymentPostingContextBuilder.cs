using OakERP.Common.Exceptions;
using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsPayable;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Infrastructure.Posting.AccountsPayable;

public sealed class ApPaymentPostingContextBuilder : IApPaymentPostingContextBuilder
{
    public Task<ApPaymentPostingContext> BuildAsync(
        ApPayment payment,
        DateOnly postingDate,
        FiscalPeriod period,
        GlPostingSettings settings,
        PostingRule rule,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(rule);

        string bankAccountNo =
            payment.BankAccount?.GlAccountNo
            ?? throw new PostingInvariantViolationException(
                "AP payment posting requires a bank account GL account."
            );

        decimal allocatedAmount = ApSettlementCalculator.GetPaymentAllocatedAmount(payment);
        decimal unappliedAmount = ApSettlementCalculator.GetPaymentUnappliedAmount(payment);

        return Task.FromResult(
            new ApPaymentPostingContext(
                payment,
                postingDate,
                period,
                settings,
                rule,
                bankAccountNo,
                allocatedAmount,
                unappliedAmount
            )
        );
    }
}
