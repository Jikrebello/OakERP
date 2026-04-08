using OakERP.Common.Enums;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Application.Posting;

internal sealed class PostingOperationSupport(
    PostingPersistenceDependencies persistenceDependencies,
    PostingRuntimeDependencies runtimeDependencies,
    PostingResultProcessor resultProcessor
)
{
    public IPostingEngine PostingEngine => runtimeDependencies.PostingEngine;

    public Task<GlPostingSettings> GetSettingsAsync(CancellationToken cancellationToken) =>
        runtimeDependencies.GlSettingsProvider.GetSettingsAsync(cancellationToken);

    public Task<PostingRule> GetActiveRuleAsync(
        DocKind docKind,
        CancellationToken cancellationToken
    ) => runtimeDependencies.PostingRuleProvider.GetActiveRuleAsync(docKind, cancellationToken);

    public async Task<FiscalPeriod> GetOpenPeriodAsync(
        DateOnly postingDate,
        CancellationToken cancellationToken
    ) =>
        await persistenceDependencies.FiscalPeriodRepository.GetOpenForDateAsync(
            postingDate,
            cancellationToken
        )
        ?? throw new InvalidOperationException(
            $"No open fiscal period exists for posting date {postingDate:yyyy-MM-dd}."
        );

    public Task ProcessPostingResultAsync(
        PostingEngineResult postingResult,
        string expectedSourceType,
        bool inventoryRowsAllowed,
        string performedBy,
        CancellationToken cancellationToken
    ) => resultProcessor.ProcessAsync(
        postingResult,
        expectedSourceType,
        inventoryRowsAllowed,
        performedBy,
        cancellationToken
    );

    public static void EnsureForceDisabled(bool force, string message)
    {
        if (force)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void EnsureDraftStatus(DocStatus status, string message)
    {
        if (status != DocStatus.Draft)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void EnsureBaseCurrency(
        string currencyCode,
        string baseCurrencyCode,
        string message
    )
    {
        if (!string.Equals(currencyCode, baseCurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(message);
        }
    }
}
