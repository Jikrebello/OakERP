using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Posting.Accounts_Receivable;

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
            ?? throw new InvalidOperationException(
                "AR receipt posting requires a bank account GL account."
            );

        decimal allocatedAmount = receipt.Allocations.Sum(x => x.AmountApplied);
        decimal unappliedAmount = receipt.Amount - allocatedAmount;

        return Task.FromResult(
            new ArReceiptPostingContext(
                receipt,
                postingDate,
                period,
                settings.BaseCurrencyCode,
                1m,
                settings,
                rule,
                bankAccountNo,
                allocatedAmount,
                unappliedAmount
            )
        );
    }
}
