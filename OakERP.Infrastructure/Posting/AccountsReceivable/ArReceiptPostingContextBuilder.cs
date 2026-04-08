using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Common.Exceptions;

namespace OakERP.Infrastructure.Posting.AccountsReceivable;

public sealed class ArReceiptPostingContextBuilder : IArReceiptPostingContextBuilder
{
    public Task<ArReceiptPostingContext> BuildAsync(
        ArReceipt receipt,
        DateOnly postingDate,
        FiscalPeriod period,
        GlPostingSettings settings,
        PostingRule rule,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(rule);

        string bankAccountNo =
            receipt.BankAccount?.GlAccountNo
            ?? throw new PostingInvariantViolationException(
                "AR receipt posting requires a bank account GL account."
            );

        return Task.FromResult(
            new ArReceiptPostingContext(receipt, postingDate, period, settings, rule, bankAccountNo)
        );
    }
}
