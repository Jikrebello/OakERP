namespace OakERP.Application.Settlements.Allocation;

internal sealed record SettlementAllocationInput(Guid InvoiceId, decimal AmountApplied);

internal sealed record SettlementAllocationFailures<TFailure>(
    TFailure DocumentUnappliedAmountExceededFailure,
    TFailure InvoiceNotFoundFailure,
    Func<string, TFailure> InvoiceRemainingAmountExceededFailureFactory
);

internal sealed class SettlementAllocationInvoiceAdapter(
    Guid invoiceId,
    string docNo,
    Func<decimal> getSettledAmount,
    Func<decimal, decimal> getRemainingAmount,
    Action<string, DateTimeOffset> touchInvoice,
    Action<decimal> updateInvoiceStatusForRemainingAmount
)
{
    public Guid InvoiceId { get; } = invoiceId;

    public string DocNo { get; } = docNo;

    public Func<decimal> GetSettledAmount { get; } = getSettledAmount;

    public Func<decimal, decimal> GetRemainingAmount { get; } = getRemainingAmount;

    public Action<string, DateTimeOffset> TouchInvoice { get; } = touchInvoice;

    public Action<decimal> UpdateInvoiceStatusForRemainingAmount { get; } =
        updateInvoiceStatusForRemainingAmount;
}

internal sealed class SettlementAllocationApplySpec<TAllocation, TFailure>(
    Func<IReadOnlyCollection<TAllocation>> getExistingAllocations,
    Func<decimal> getDocumentUnappliedAmount,
    Action<string, DateTimeOffset> touchDocument,
    Func<Guid, SettlementAllocationInvoiceAdapter?> findInvoice,
    Func<SettlementAllocationInput, DateOnly, TAllocation> createAllocation,
    Func<TAllocation, Task> persistAllocationAsync,
    SettlementAllocationFailures<TFailure> failures
)
{
    public Func<IReadOnlyCollection<TAllocation>> GetExistingAllocations { get; } =
        getExistingAllocations;

    public Func<decimal> GetDocumentUnappliedAmount { get; } = getDocumentUnappliedAmount;

    public Action<string, DateTimeOffset> TouchDocument { get; } = touchDocument;

    public Func<Guid, SettlementAllocationInvoiceAdapter?> FindInvoice { get; } = findInvoice;

    public Func<SettlementAllocationInput, DateOnly, TAllocation> CreateAllocation { get; } =
        createAllocation;

    public Func<TAllocation, Task> PersistAllocationAsync { get; } = persistAllocationAsync;

    public SettlementAllocationFailures<TFailure> Failures { get; } = failures;
}
