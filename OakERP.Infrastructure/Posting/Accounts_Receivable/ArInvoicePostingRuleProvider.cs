using OakERP.Common.Enums;
using OakERP.Domain.Posting;

namespace OakERP.Infrastructure.Posting.Accounts_Receivable;

public sealed class ArInvoicePostingRuleProvider : IPostingRuleProvider
{
    public Task<PostingRule> GetActiveRuleAsync(
        DocKind docKind,
        CancellationToken cancellationToken = default
    )
    {
        if (docKind != DocKind.ArInvoice)
        {
            throw new NotSupportedException(
                $"Posting rule for document kind '{docKind}' is not supported."
            );
        }

        var rule = new PostingRule
        {
            DocKind = DocKind.ArInvoice,
            Name = "AR Invoice Runtime Rule",
            IsActive = true,
            Lines =
            [
                new PostingRuleLine
                {
                    Side = RuleSide.Debit,
                    AccountKey = AccountKey.AccountsReceivable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                },
                new PostingRuleLine
                {
                    Side = RuleSide.Credit,
                    AccountKey = AccountKey.Revenue,
                    AmountSource = AmountSource.LineNet,
                    Scope = PostingRuleScopes.Line,
                },
                new PostingRuleLine
                {
                    Side = RuleSide.Credit,
                    AccountKey = AccountKey.TaxOutput,
                    AmountSource = AmountSource.HeaderTaxTotal,
                    Scope = PostingRuleScopes.Tax,
                },
                new PostingRuleLine
                {
                    Side = RuleSide.Debit,
                    AccountKey = AccountKey.Cogs,
                    AmountSource = AmountSource.LineCogsValue,
                    Scope = PostingRuleScopes.LineStock,
                },
                new PostingRuleLine
                {
                    Side = RuleSide.Credit,
                    AccountKey = AccountKey.InventoryAsset,
                    AmountSource = AmountSource.LineCogsValue,
                    Scope = PostingRuleScopes.LineStock,
                },
            ],
        };

        return Task.FromResult(rule);
    }
}
