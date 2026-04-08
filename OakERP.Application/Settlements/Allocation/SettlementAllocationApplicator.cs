namespace OakERP.Application.Settlements.Allocation;

internal static class SettlementAllocationApplicator
{
    public static async Task<(
        TFailure? failure,
        IReadOnlyDictionary<Guid, decimal> settledAmounts,
        IReadOnlyList<TAllocation> allocations
    )> ApplyAsync<TAllocation, TFailure>(
        IReadOnlyList<SettlementAllocationInput> allocationInputs,
        DateOnly allocationDate,
        string performedBy,
        DateTimeOffset updatedAt,
        SettlementAllocationApplySpec<TAllocation, TFailure> spec
    )
    {
        List<TAllocation> allocations = [.. spec.GetExistingAllocations()];
        Dictionary<Guid, decimal> settledAmounts = [];

        if (allocationInputs.Count == 0)
        {
            return (default, settledAmounts, allocations);
        }

        decimal requestedTotal = allocationInputs.Sum(input => input.AmountApplied);
        decimal documentUnappliedAmount = spec.GetDocumentUnappliedAmount();

        if (requestedTotal > documentUnappliedAmount)
        {
            return (
                spec.Failures.DocumentUnappliedAmountExceededFailure,
                settledAmounts,
                allocations
            );
        }

        spec.TouchDocument(performedBy, updatedAt);

        foreach (SettlementAllocationInput input in allocationInputs)
        {
            SettlementAllocationInvoiceAdapter? invoice = spec.FindInvoice(input.InvoiceId);
            if (invoice is null)
            {
                return (spec.Failures.InvoiceNotFoundFailure, settledAmounts, allocations);
            }

            Guid invoiceId = invoice.InvoiceId;
            if (!settledAmounts.TryGetValue(invoiceId, out decimal currentSettledAmount))
            {
                currentSettledAmount = invoice.GetSettledAmount();
                settledAmounts[invoiceId] = currentSettledAmount;
            }

            decimal invoiceRemainingAmount = invoice.GetRemainingAmount(currentSettledAmount);
            decimal amountApplied = input.AmountApplied;
            if (amountApplied > invoiceRemainingAmount)
            {
                return (
                    spec.Failures.InvoiceRemainingAmountExceededFailureFactory(invoice.DocNo),
                    settledAmounts,
                    allocations
                );
            }

            TAllocation allocation = spec.CreateAllocation(input, allocationDate);

            await spec.PersistAllocationAsync(allocation);
            allocations.Add(allocation);

            decimal remainingAfterAllocation = invoiceRemainingAmount - amountApplied;
            settledAmounts[invoiceId] = currentSettledAmount + amountApplied;
            invoice.TouchInvoice(performedBy, updatedAt);
            invoice.UpdateInvoiceStatusForRemainingAmount(remainingAfterAllocation);
        }

        return (default, settledAmounts, allocations);
    }
}
