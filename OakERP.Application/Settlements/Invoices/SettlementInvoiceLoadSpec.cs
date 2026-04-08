using OakERP.Common.Enums;

namespace OakERP.Application.Settlements.Invoices;

internal sealed record SettlementInvoiceSnapshot(
    Guid Id,
    string DocNo,
    DocStatus DocStatus,
    Guid PartyId,
    string CurrencyCode,
    decimal RemainingAmount
);

internal sealed record SettlementInvoiceLoadExpectations(
    Guid ExpectedPartyId,
    string ExpectedCurrencyCode
);

internal sealed record SettlementInvoiceLoadFailures<TFailure>(
    TFailure InvoicesNotFoundFailure,
    TFailure InvoiceNotPostedFailure,
    TFailure PartyMismatchFailure,
    TFailure CurrencyMismatchFailure,
    Func<string, TFailure> NoRemainingBalanceFailureFactory
);

internal sealed class SettlementInvoiceLoadSpec<TInvoice, TFailure>(
    Func<
        IReadOnlyCollection<Guid>,
        CancellationToken,
        Task<IReadOnlyList<TInvoice>>
    > loadInvoicesAsync,
    Func<TInvoice, SettlementInvoiceSnapshot> describeInvoice,
    SettlementInvoiceLoadExpectations expectations,
    SettlementInvoiceLoadFailures<TFailure> failures
)
{
    public Func<
        IReadOnlyCollection<Guid>,
        CancellationToken,
        Task<IReadOnlyList<TInvoice>>
    > LoadInvoicesAsync { get; } = loadInvoicesAsync;

    public Func<TInvoice, SettlementInvoiceSnapshot> DescribeInvoice { get; } = describeInvoice;

    public SettlementInvoiceLoadExpectations Expectations { get; } = expectations;

    public SettlementInvoiceLoadFailures<TFailure> Failures { get; } = failures;
}
