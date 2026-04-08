using OakERP.Common.Enums;

namespace OakERP.Application.Settlements.Documents;

internal sealed record SettlementDocumentPartySnapshot(Guid PartyId, bool IsActive);

internal sealed record SettlementDocumentBankAccountSnapshot(
    Guid BankAccountId,
    bool IsActive,
    string CurrencyCode
);

internal static class SettlementDocumentPreconditions
{
    public static async Task<(SettlementDocumentPartySnapshot? party, TResult? failure)> LoadActivePartyAsync<
        TParty,
        TResult
    >(
        Func<CancellationToken, Task<TParty?>> loadPartyAsync,
        Func<TParty, SettlementDocumentPartySnapshot> describeParty,
        TResult notFoundFailure,
        TResult inactiveFailure,
        CancellationToken cancellationToken
    )
        where TParty : class
    {
        TParty? party = await loadPartyAsync(cancellationToken);
        if (party is null)
        {
            return (null, notFoundFailure);
        }

        SettlementDocumentPartySnapshot snapshot = describeParty(party);
        return snapshot.IsActive ? (snapshot, default) : (null, inactiveFailure);
    }

    public static async Task<(
        SettlementDocumentBankAccountSnapshot? bankAccount,
        TResult? failure
    )> LoadActiveBankAccountAsync<TBankAccount, TResult>(
        Func<CancellationToken, Task<TBankAccount?>> loadBankAccountAsync,
        Func<TBankAccount, SettlementDocumentBankAccountSnapshot> describeBankAccount,
        TResult notFoundFailure,
        TResult inactiveFailure,
        CancellationToken cancellationToken
    )
        where TBankAccount : class
    {
        TBankAccount? bankAccount = await loadBankAccountAsync(cancellationToken);
        if (bankAccount is null)
        {
            return (null, notFoundFailure);
        }

        SettlementDocumentBankAccountSnapshot snapshot = describeBankAccount(bankAccount);
        return snapshot.IsActive ? (snapshot, default) : (null, inactiveFailure);
    }

    public static TResult? EnsureCurrency<TResult>(
        string actualCurrencyCode,
        string expectedCurrencyCode,
        Func<string, string, bool> matchesCurrency,
        TResult failure
    ) =>
        matchesCurrency(actualCurrencyCode, expectedCurrencyCode) ? default : failure;

    public static TResult? EnsureDraftStatus<TResult>(DocStatus docStatus, TResult failure) =>
        docStatus == DocStatus.Draft ? default : failure;

    public static async Task<TResult?> EnsureDocumentNumberAvailableAsync<TResult>(
        Func<CancellationToken, Task<bool>> existsAsync,
        TResult duplicateFailure,
        CancellationToken cancellationToken
    ) => await existsAsync(cancellationToken) ? duplicateFailure : default;
}
