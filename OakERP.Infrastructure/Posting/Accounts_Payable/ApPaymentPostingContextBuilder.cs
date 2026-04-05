using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Payable;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Posting.Accounts_Payable;

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
            ?? throw new InvalidOperationException(
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
