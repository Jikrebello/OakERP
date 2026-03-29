using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Posting.Accounts_Receivable;

public sealed class ArInvoicePostingEngine : IPostingEngine
{
    private const string SourceType = "ARINV";

    public PostingEngineResult PostArInvoice(ArInvoicePostingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var rule = context.Rule ?? throw new InvalidOperationException("Posting rule is required.");
        var invoice = context.Invoice;

        var glEntries = new List<GlEntryModel>();

        var arRule = FindRuleLine(
            rule,
            scope: "Header",
            side: RuleSide.Debit,
            accountKey: AccountKey.AccountsReceivable,
            amountSource: AmountSource.HeaderDocTotal
        );

        glEntries.Add(
            new GlEntryModel(
                context.PostingDate,
                context.Period.Id,
                ResolveHeaderAccountNo(arRule.AccountKey, context),
                invoice.DocTotal,
                0m,
                SourceType,
                invoice.Id,
                invoice.DocNo,
                $"AR invoice {invoice.DocNo}"
            )
        );

        var revenueRule = FindRuleLine(
            rule,
            scope: "Line",
            side: RuleSide.Credit,
            accountKey: AccountKey.Revenue,
            amountSource: AmountSource.LineNet
        );

        foreach (var line in context.Lines.OrderBy(x => x.LineNo))
        {
            var revenueAccountNo = ResolveRevenueAccountNo(line, context);

            glEntries.Add(
                new GlEntryModel(
                    context.PostingDate,
                    context.Period.Id,
                    revenueAccountNo,
                    0m,
                    line.LineTotal,
                    SourceType,
                    invoice.Id,
                    invoice.DocNo,
                    $"AR invoice {invoice.DocNo} line {line.LineNo}"
                )
            );
        }

        if (invoice.TaxTotal > 0m)
        {
            var taxRule = FindRuleLine(
                rule,
                scope: "Tax",
                side: RuleSide.Credit,
                accountKey: AccountKey.TaxOutput,
                amountSource: AmountSource.HeaderTaxTotal
            );

            glEntries.Add(
                new GlEntryModel(
                    context.PostingDate,
                    context.Period.Id,
                    ResolveHeaderAccountNo(taxRule.AccountKey, context),
                    0m,
                    invoice.TaxTotal,
                    SourceType,
                    invoice.Id,
                    invoice.DocNo,
                    $"AR invoice {invoice.DocNo} tax"
                )
            );
        }

        return new PostingEngineResult(glEntries, []);
    }

    private static PostingRuleLine FindRuleLine(
        PostingRule rule,
        string scope,
        RuleSide side,
        AccountKey accountKey,
        AmountSource amountSource
    ) =>
        rule.Lines.SingleOrDefault(x =>
            string.Equals(x.Scope, scope, StringComparison.OrdinalIgnoreCase)
            && x.Side == side
            && x.AccountKey == accountKey
            && x.AmountSource == amountSource
        )
        ?? throw new InvalidOperationException(
            $"Posting rule '{rule.Name}' is missing the expected '{scope}' line for account key '{accountKey}'."
        );

    private static string ResolveHeaderAccountNo(AccountKey accountKey, ArInvoicePostingContext context) =>
        accountKey switch
        {
            AccountKey.AccountsReceivable => context.Settings.ArControlAccountNo,
            AccountKey.TaxOutput => context.Settings.DefaultTaxOutputAccountNo,
            _ => throw new InvalidOperationException(
                $"Header account key '{accountKey}' is not supported for AR invoice Slice 1A."
            ),
        };

    private static string ResolveRevenueAccountNo(
        ArInvoiceLine line,
        ArInvoicePostingContext context
    )
    {
        var accountNo =
            FirstNonBlank(
                line.RevenueAccount,
                line.Item?.DefaultRevenueAccountNo,
                line.Item?.Category?.RevenueAccount,
                context.Settings.DefaultRevenueAccountNo
            )
            ?? throw new InvalidOperationException(
                $"No revenue account could be resolved for AR invoice line {line.LineNo}."
            );

        return accountNo;
    }

    private static string? FirstNonBlank(params string?[] values) =>
        values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
}
