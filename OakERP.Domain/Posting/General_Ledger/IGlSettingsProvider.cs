namespace OakERP.Domain.Posting.General_Ledger;

public interface IGlSettingsProvider
{
    /// <summary>
    /// Asynchronously retrieves the current general ledger posting settings.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current general ledger posting
    /// settings.</returns>
    Task<GlPostingSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
}
