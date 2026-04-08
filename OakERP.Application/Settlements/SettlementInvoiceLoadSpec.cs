using OakERP.Common.Enums;

namespace OakERP.Application.Settlements;

internal sealed class SettlementInvoiceLoadSpec<TInvoice, TFailure>(
    Func<IReadOnlyCollection<Guid>, CancellationToken, Task<IReadOnlyList<TInvoice>>> loadInvoicesAsync,
    Func<TInvoice, Guid> getInvoiceId,
    Func<TInvoice, string> getDocNo,
    Func<TInvoice, DocStatus> getDocStatus,
    Func<TInvoice, Guid> getPartyId,
    Func<TInvoice, string> getCurrencyCode,
    Func<TInvoice, decimal> getRemainingAmount,
    Guid expectedPartyId,
    string expectedCurrencyCode,
    TFailure invoicesNotFoundFailure,
    TFailure invoiceNotPostedFailure,
    TFailure partyMismatchFailure,
    TFailure currencyMismatchFailure,
    Func<string, TFailure> noRemainingBalanceFailureFactory
)
{
    public Func<IReadOnlyCollection<Guid>, CancellationToken, Task<IReadOnlyList<TInvoice>>> LoadInvoicesAsync { get; } =
        loadInvoicesAsync;

    public Func<TInvoice, Guid> GetInvoiceId { get; } = getInvoiceId;

    public Func<TInvoice, string> GetDocNo { get; } = getDocNo;

    public Func<TInvoice, DocStatus> GetDocStatus { get; } = getDocStatus;

    public Func<TInvoice, Guid> GetPartyId { get; } = getPartyId;

    public Func<TInvoice, string> GetCurrencyCode { get; } = getCurrencyCode;

    public Func<TInvoice, decimal> GetRemainingAmount { get; } = getRemainingAmount;

    public Guid ExpectedPartyId { get; } = expectedPartyId;

    public string ExpectedCurrencyCode { get; } = expectedCurrencyCode;

    public TFailure InvoicesNotFoundFailure { get; } = invoicesNotFoundFailure;

    public TFailure InvoiceNotPostedFailure { get; } = invoiceNotPostedFailure;

    public TFailure PartyMismatchFailure { get; } = partyMismatchFailure;

    public TFailure CurrencyMismatchFailure { get; } = currencyMismatchFailure;

    public Func<string, TFailure> NoRemainingBalanceFailureFactory { get; } =
        noRemainingBalanceFailureFactory;
}
