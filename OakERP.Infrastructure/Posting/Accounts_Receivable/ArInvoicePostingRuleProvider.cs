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
        return Task.FromResult(
            docKind switch
            {
                DocKind.ArInvoice => CreateArInvoiceRule(),
                DocKind.ArReceipt => CreateArReceiptRule(),
                _ => throw new NotSupportedException(
                    $"Posting rule for document kind '{docKind}' is not supported."
                ),
            }
        );
    }

    private static PostingRule CreateArInvoiceRule() =>
        new()
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

    private static PostingRule CreateArReceiptRule() =>
        new()
        {
            DocKind = DocKind.ArReceipt,
            Name = "AR Receipt Runtime Rule",
            IsActive = true,
            Lines =
            [
                new PostingRuleLine
                {
                    Side = RuleSide.Debit,
                    AccountKey = AccountKey.Bank,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                },
                new PostingRuleLine
                {
                    Side = RuleSide.Credit,
                    AccountKey = AccountKey.AccountsReceivable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                },
            ],
        };
}
