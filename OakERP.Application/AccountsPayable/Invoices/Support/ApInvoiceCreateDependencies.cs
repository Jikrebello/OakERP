using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;

namespace OakERP.Application.AccountsPayable.Invoices.Support;

public sealed class ApInvoiceCreateDependencies(
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    ICurrencyRepository currencyRepository,
    IGlAccountRepository glAccountRepository
)
{
    public IApInvoiceRepository ApInvoiceRepository { get; } = apInvoiceRepository;

    public IVendorRepository VendorRepository { get; } = vendorRepository;

    public ICurrencyRepository CurrencyRepository { get; } = currencyRepository;

    public IGlAccountRepository GlAccountRepository { get; } = glAccountRepository;
}
