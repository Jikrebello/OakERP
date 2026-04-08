using Microsoft.EntityFrameworkCore;
using OakERP.Common.Exceptions;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting.Inventory;
using OakERP.Domain.RepositoryInterfaces.Inventory;

namespace OakERP.Infrastructure.Posting.Inventory;

public sealed class MovingAverageInventoryCostService(
    IInventoryLedgerRepository inventoryLedgerRepository
) : IInventoryCostService
{
    public async Task<decimal> GetUnitCostForSaleAsync(
        Guid itemId,
        Guid locationId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default
    )
    {
        List<InventoryLedger> ledgers = await inventoryLedgerRepository
            .QueryNoTracking()
            .Where(x => x.ItemId == itemId && x.LocationId == locationId && x.TrxDate <= asOfDate)
            .OrderBy(x => x.TrxDate)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        decimal runningQty = 0m;
        decimal runningValue = 0m;
        decimal? lastKnownUnitCost = null;

        foreach (InventoryLedger ledger in ledgers)
        {
            runningQty += ledger.Qty;
            runningValue += ledger.ValueChange;

            if (runningQty != 0m)
            {
                lastKnownUnitCost = Math.Round(
                    runningValue / runningQty,
                    4,
                    MidpointRounding.AwayFromZero
                );
            }
        }

        return lastKnownUnitCost
            ?? throw new PostingInvariantViolationException(
                $"No prior cost basis exists for item '{itemId}' at location '{locationId}' as of {asOfDate:yyyy-MM-dd}."
            );
    }
}
