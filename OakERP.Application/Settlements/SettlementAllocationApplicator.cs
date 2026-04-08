namespace OakERP.Application.Settlements;

internal static class SettlementAllocationApplicator
{
    public static async Task<(
        TFailure? failure,
        IReadOnlyDictionary<Guid, decimal> settledAmounts,
        IReadOnlyList<TAllocation> allocations
    )> ApplyAsync<TDocument, TInvoice, TAllocation, TAllocationInput, TFailure>(
        TDocument document,
        Dictionary<Guid, TInvoice> invoices,
        IReadOnlyList<TAllocationInput> allocationInputs,
        DateOnly allocationDate,
        string performedBy,
        SettlementAllocationApplySpec<TDocument, TInvoice, TAllocation, TAllocationInput, TFailure> spec
    )
    {
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
        List<TAllocation> allocations = [.. spec.GetExistingAllocations(document)];
        Dictionary<Guid, decimal> settledAmounts = invoices.ToDictionary(
            x => x.Key,
            x => spec.GetSettledAmount(x.Value)
        );

        if (allocationInputs.Count == 0)
        {
            return (default, settledAmounts, allocations);
        }

        decimal requestedTotal = allocationInputs.Sum(spec.GetInputAmountApplied);
        decimal documentUnappliedAmount = spec.GetDocumentUnappliedAmount(document);

        if (requestedTotal > documentUnappliedAmount)
        {
            return (
                spec.DocumentUnappliedAmountExceededFailure,
                settledAmounts,
                allocations
            );
        }

        spec.TouchDocument(document, performedBy, updatedAt);

        foreach (TAllocationInput input in allocationInputs)
        {
            if (!invoices.TryGetValue(spec.GetInputInvoiceId(input), out TInvoice? invoice))
            {
                return (spec.InvoiceNotFoundFailure, settledAmounts, allocations);
            }

            Guid invoiceId = spec.GetInvoiceId(invoice);
            decimal currentSettledAmount = settledAmounts[invoiceId];
            decimal invoiceRemainingAmount = spec.GetRemainingAmount(invoice, currentSettledAmount);
            decimal amountApplied = spec.GetInputAmountApplied(input);
            if (amountApplied > invoiceRemainingAmount)
            {
                return (
                    spec.InvoiceRemainingAmountExceededFailureFactory(spec.GetDocNo(invoice)),
                    settledAmounts,
                    allocations
                );
            }

            TAllocation allocation = spec.CreateAllocation(
                spec.GetDocumentId(document),
                invoiceId,
                allocationDate,
                amountApplied
            );

            await spec.PersistAllocationAsync(allocation);
            allocations.Add(allocation);

            decimal remainingAfterAllocation = invoiceRemainingAmount - amountApplied;
            settledAmounts[invoiceId] = currentSettledAmount + amountApplied;
            spec.TouchInvoice(invoice, performedBy, updatedAt);
            spec.UpdateInvoiceStatusForRemainingAmount(invoice, remainingAfterAllocation);
        }

        return (default, settledAmounts, allocations);
    }
}
