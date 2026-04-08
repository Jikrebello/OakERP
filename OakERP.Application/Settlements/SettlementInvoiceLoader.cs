namespace OakERP.Application.Settlements;

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
            return (null, spec.InvoicesNotFoundFailure);
        }

        foreach (TInvoice invoice in invoices)
        {
            if (spec.GetDocStatus(invoice) != Common.Enums.DocStatus.Posted)
            {
                return (null, spec.InvoiceNotPostedFailure);
            }

            if (spec.GetPartyId(invoice) != spec.ExpectedPartyId)
            {
                return (null, spec.PartyMismatchFailure);
            }

            if (
                !string.Equals(
                    spec.GetCurrencyCode(invoice),
                    spec.ExpectedCurrencyCode,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return (null, spec.CurrencyMismatchFailure);
            }

            if (spec.GetRemainingAmount(invoice) <= 0m)
            {
                return (null, spec.NoRemainingBalanceFailureFactory(spec.GetDocNo(invoice)));
            }
        }

        return (invoices.ToDictionary(spec.GetInvoiceId), default);
    }
}
