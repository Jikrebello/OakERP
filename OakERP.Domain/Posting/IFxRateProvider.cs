namespace OakERP.Domain.Posting;

public interface IFxRateProvider
{
    /// <summary>
    /// Asynchronously retrieves the exchange rate from the specified currency to the base currency for a given date.
    /// </summary>
    /// <param name="fromCurrencyCode">The three-letter ISO currency code representing the source currency. Cannot be null or empty.</param>
    /// <param name="onDate">The date for which to retrieve the exchange rate.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the exchange rate from the specified
    /// currency to the base currency as of the specified date.</returns>
    Task<decimal> GetRateToBaseAsync(
        string fromCurrencyCode,
        DateOnly onDate,
        CancellationToken cancellationToken = default
    );
}
