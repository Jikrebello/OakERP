using OakERP.Common.Enums;
using OakERP.Common.Exceptions;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.Posting.Inventory;

namespace OakERP.Infrastructure.Posting.AccountsReceivable;

public sealed class ArInvoicePostingContextBuilder(IInventoryCostService inventoryCostService)
    : IArInvoicePostingContextBuilder
{
    public async Task<ArInvoicePostingContext> BuildAsync(
        ArInvoice invoice,
        DateOnly postingDate,
        FiscalPeriod period,
        GlPostingSettings settings,
        PostingRule rule,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(rule);

        var lines = new List<ArInvoicePostingLineContext>();

        foreach (ArInvoiceLine line in invoice.Lines.OrderBy(x => x.LineNo))
        {
            bool isStock = line.Item?.Type == ItemType.Stock;
            string revenueAccountNo =
                FirstNonBlank(
                    line.RevenueAccount,
                    line.Item?.DefaultRevenueAccountNo,
                    line.Item?.Category?.RevenueAccount,
                    settings.DefaultRevenueAccountNo
                )
                ?? throw new PostingInvariantViolationException(
                    $"No revenue account could be resolved for AR invoice line {line.LineNo}."
                );

            if (!isStock)
            {
                lines.Add(
                    new ArInvoicePostingLineContext(
                        line,
                        false,
                        revenueAccountNo,
                        null,
                        null,
                        null,
                        null,
                        null
                    )
                );
                continue;
            }

            Guid locationId =
                line.LocationId
                ?? throw new PostingInvariantViolationException(
                    $"Stock AR invoice line {line.LineNo} requires a location."
                );

            string cogsAccountNo =
                FirstNonBlank(line.Item?.Category?.CogsAccount, settings.DefaultCogsAccountNo)
                ?? throw new PostingInvariantViolationException(
                    $"No COGS account could be resolved for AR invoice line {line.LineNo}."
                );

            string inventoryAssetAccountNo =
                FirstNonBlank(
                    line.Item?.Category?.InventoryAccount,
                    settings.DefaultInventoryAssetAccountNo
                )
                ?? throw new PostingInvariantViolationException(
                    $"No inventory asset account could be resolved for AR invoice line {line.LineNo}."
                );

            decimal unitCost = await inventoryCostService.GetUnitCostForSaleAsync(
                line.ItemId
                    ?? throw new PostingInvariantViolationException(
                        $"Stock AR invoice line {line.LineNo} requires an item."
                    ),
                locationId,
                postingDate,
                cancellationToken
            );

            decimal lineCogsValue = Math.Round(
                line.Qty * unitCost,
                2,
                MidpointRounding.AwayFromZero
            );

            lines.Add(
                new ArInvoicePostingLineContext(
                    line,
                    true,
                    revenueAccountNo,
                    locationId,
                    cogsAccountNo,
                    inventoryAssetAccountNo,
                    unitCost,
                    lineCogsValue
                )
            );
        }

        return new ArInvoicePostingContext(
            invoice,
            lines,
            postingDate,
            period,
            settings.BaseCurrencyCode,
            1m,
            settings,
            rule
        );
    }

    private static string? FirstNonBlank(params string?[] values) =>
        values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
}
