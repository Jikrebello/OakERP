namespace OakERP.Application.Settlements;

internal sealed class SettlementAllocationApplySpec<
    TDocument,
    TInvoice,
    TAllocation,
    TAllocationInput,
    TFailure
>(
    Func<TDocument, Guid> getDocumentId,
    Func<TDocument, IReadOnlyCollection<TAllocation>> getExistingAllocations,
    Func<TDocument, decimal> getDocumentUnappliedAmount,
    Action<TDocument, string, DateTimeOffset> touchDocument,
    Func<TInvoice, Guid> getInvoiceId,
    Func<TInvoice, string> getDocNo,
    Func<TInvoice, decimal> getSettledAmount,
    Func<TInvoice, decimal, decimal> getRemainingAmount,
    Action<TInvoice, string, DateTimeOffset> touchInvoice,
    Action<TInvoice, decimal> updateInvoiceStatusForRemainingAmount,
    Func<TAllocationInput, Guid> getInputInvoiceId,
    Func<TAllocationInput, decimal> getInputAmountApplied,
    Func<Guid, Guid, DateOnly, decimal, TAllocation> createAllocation,
    Func<TAllocation, Task> persistAllocationAsync,
    TFailure documentUnappliedAmountExceededFailure,
    TFailure invoiceNotFoundFailure,
    Func<string, TFailure> invoiceRemainingAmountExceededFailureFactory
)
{
    public Func<TDocument, Guid> GetDocumentId { get; } = getDocumentId;

    public Func<TDocument, IReadOnlyCollection<TAllocation>> GetExistingAllocations { get; } =
        getExistingAllocations;

    public Func<TDocument, decimal> GetDocumentUnappliedAmount { get; } = getDocumentUnappliedAmount;

    public Action<TDocument, string, DateTimeOffset> TouchDocument { get; } = touchDocument;

    public Func<TInvoice, Guid> GetInvoiceId { get; } = getInvoiceId;

    public Func<TInvoice, string> GetDocNo { get; } = getDocNo;

    public Func<TInvoice, decimal> GetSettledAmount { get; } = getSettledAmount;

    public Func<TInvoice, decimal, decimal> GetRemainingAmount { get; } = getRemainingAmount;

    public Action<TInvoice, string, DateTimeOffset> TouchInvoice { get; } = touchInvoice;

    public Action<TInvoice, decimal> UpdateInvoiceStatusForRemainingAmount { get; } =
        updateInvoiceStatusForRemainingAmount;

    public Func<TAllocationInput, Guid> GetInputInvoiceId { get; } = getInputInvoiceId;

    public Func<TAllocationInput, decimal> GetInputAmountApplied { get; } = getInputAmountApplied;

    public Func<Guid, Guid, DateOnly, decimal, TAllocation> CreateAllocation { get; } =
        createAllocation;

    public Func<TAllocation, Task> PersistAllocationAsync { get; } = persistAllocationAsync;

    public TFailure DocumentUnappliedAmountExceededFailure { get; } =
        documentUnappliedAmountExceededFailure;

    public TFailure InvoiceNotFoundFailure { get; } = invoiceNotFoundFailure;

    public Func<string, TFailure> InvoiceRemainingAmountExceededFailureFactory { get; } =
        invoiceRemainingAmountExceededFailureFactory;
}
