using OakERP.Common.Enums;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;

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
        var inventoryMovements = new List<InventoryMovementModel>();

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

        foreach (var postingLine in context.Lines.OrderBy(x => x.Line.LineNo))
        {
            glEntries.Add(
                new GlEntryModel(
                    context.PostingDate,
                    context.Period.Id,
                    postingLine.RevenueAccountNo,
                    0m,
                    postingLine.Line.LineTotal,
                    SourceType,
                    invoice.Id,
                    invoice.DocNo,
                    $"AR invoice {invoice.DocNo} line {postingLine.Line.LineNo}"
                )
            );

            if (!postingLine.IsStock)
            {
                continue;
            }

            string cogsAccountNo =
                postingLine.CogsAccountNo
                ?? throw new InvalidOperationException(
                    $"Stock AR invoice line {postingLine.Line.LineNo} is missing a COGS account."
                );
            string inventoryAssetAccountNo =
                postingLine.InventoryAssetAccountNo
                ?? throw new InvalidOperationException(
                    $"Stock AR invoice line {postingLine.Line.LineNo} is missing an inventory asset account."
                );
            Guid locationId =
                postingLine.LocationId
                ?? throw new InvalidOperationException(
                    $"Stock AR invoice line {postingLine.Line.LineNo} is missing a location."
                );
            decimal unitCost =
                postingLine.UnitCost
                ?? throw new InvalidOperationException(
                    $"Stock AR invoice line {postingLine.Line.LineNo} is missing a unit cost."
                );
            decimal lineCogsValue =
                postingLine.LineCogsValue
                ?? throw new InvalidOperationException(
                    $"Stock AR invoice line {postingLine.Line.LineNo} is missing a COGS value."
                );

            FindRuleLine(
                rule,
                scope: "Line.Stock",
                side: RuleSide.Debit,
                accountKey: AccountKey.Cogs,
                amountSource: AmountSource.LineCogsValue
            );

            FindRuleLine(
                rule,
                scope: "Line.Stock",
                side: RuleSide.Credit,
                accountKey: AccountKey.InventoryAsset,
                amountSource: AmountSource.LineCogsValue
            );

            glEntries.Add(
                new GlEntryModel(
                    context.PostingDate,
                    context.Period.Id,
                    cogsAccountNo,
                    lineCogsValue,
                    0m,
                    SourceType,
                    invoice.Id,
                    invoice.DocNo,
                    $"AR invoice {invoice.DocNo} line {postingLine.Line.LineNo} COGS"
                )
            );

            glEntries.Add(
                new GlEntryModel(
                    context.PostingDate,
                    context.Period.Id,
                    inventoryAssetAccountNo,
                    0m,
                    lineCogsValue,
                    SourceType,
                    invoice.Id,
                    invoice.DocNo,
                    $"AR invoice {invoice.DocNo} line {postingLine.Line.LineNo} inventory"
                )
            );

            inventoryMovements.Add(
                new InventoryMovementModel(
                    context.PostingDate,
                    postingLine.Line.ItemId
                        ?? throw new InvalidOperationException(
                            $"Stock AR invoice line {postingLine.Line.LineNo} is missing an item."
                        ),
                    locationId,
                    InventoryTransactionType.SalesCogs,
                    -postingLine.Line.Qty,
                    unitCost,
                    -lineCogsValue,
                    SourceType,
                    invoice.Id,
                    $"AR invoice {invoice.DocNo} line {postingLine.Line.LineNo}"
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

        return new PostingEngineResult(glEntries, inventoryMovements);
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

}
