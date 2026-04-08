using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Domain.Posting;

public sealed record AccountResolutionContext(
    GlPostingSettings Settings,
    ArInvoice? Invoice = null,
    ArInvoiceLine? InvoiceLine = null,
    Item? Item = null,
    ItemCategory? ItemCategory = null,
    TaxRate? TaxRate = null,
    BankAccount? BankAccount = null
);

public interface IAccountResolver
{
    /// <summary>
    /// Asynchronously resolves the specified account key to its corresponding account identifier.
    /// </summary>
    /// <param name="key">The account key to resolve. Cannot be null.</param>
    /// <param name="context">The resolution context containing additional information or options for the account resolution process. Cannot
    /// be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the resolved account identifier as a
    /// string.</returns>
    Task<string> ResolveAsync(
        AccountKey key,
        AccountResolutionContext context,
        CancellationToken cancellationToken = default
    );
}
