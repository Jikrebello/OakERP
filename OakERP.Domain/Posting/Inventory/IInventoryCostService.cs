namespace OakERP.Domain.Posting.Inventory;

public interface IInventoryCostService
{
    /// <summary>
    /// Asynchronously retrieves the unit cost of a specified item at a given location as of a particular date.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item for which to retrieve the unit cost.</param>
    /// <param name="locationId">The unique identifier of the location where the item's cost is to be determined.</param>
    /// <param name="asOfDate">The date for which the unit cost should be calculated.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unit cost of the item at the
    /// specified location and date.</returns>
    Task<decimal> GetUnitCostForSaleAsync(
        Guid itemId,
        Guid locationId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default
    );
}