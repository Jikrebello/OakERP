using OakERP.Common.Enums;

namespace OakERP.Domain.Posting;

public interface IPostingRuleProvider
{
    /// <summary>
    /// Asynchronously retrieves the active posting rule for the specified document kind.
    /// </summary>
    /// <param name="docKind">The type of document for which to retrieve the active posting rule.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the active posting rule for the
    /// specified document kind, or null if no active rule exists.</returns>
    Task<PostingRule> GetActiveRuleAsync(
        DocKind docKind,
        CancellationToken cancellationToken = default
    );
}