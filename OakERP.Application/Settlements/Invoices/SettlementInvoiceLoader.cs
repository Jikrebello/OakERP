namespace OakERP.Application.Settlements.Invoices;

internal static class SettlementInvoiceLoader
{
    public static async Task<(Dictionary<Guid, TInvoice>? invoices, TFailure? failure)> LoadAsync<
        TInvoice,
        TFailure
    >(
        IReadOnlyCollection<Guid> invoiceIds,
        SettlementInvoiceLoadSpec<TInvoice, TFailure> spec,
        CancellationToken cancellationToken
    )
    {
        if (invoiceIds.Count == 0)
        {
            return ([], default);
        }

        IReadOnlyList<TInvoice> invoices = await spec.LoadInvoicesAsync(
            invoiceIds,
            cancellationToken
        );

        if (invoices.Count != invoiceIds.Count)
        {
            return (null, spec.Failures.InvoicesNotFoundFailure);
        }

        foreach (TInvoice invoice in invoices)
        {
            SettlementInvoiceSnapshot snapshot = spec.DescribeInvoice(invoice);

            if (snapshot.DocStatus != Common.Enums.DocStatus.Posted)
            {
                return (null, spec.Failures.InvoiceNotPostedFailure);
            }

            if (snapshot.PartyId != spec.Expectations.ExpectedPartyId)
            {
                return (null, spec.Failures.PartyMismatchFailure);
            }

            if (
                !string.Equals(
                    snapshot.CurrencyCode,
                    spec.Expectations.ExpectedCurrencyCode,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return (null, spec.Failures.CurrencyMismatchFailure);
            }

            if (snapshot.RemainingAmount <= 0m)
            {
                return (null, spec.Failures.NoRemainingBalanceFailureFactory(snapshot.DocNo));
            }
        }

        return (invoices.ToDictionary(invoice => spec.DescribeInvoice(invoice).Id), default);
    }
}
