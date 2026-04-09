using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.Inventory;

namespace OakERP.Application.AccountsReceivable.Invoices.Support;

public sealed class ArInvoiceCreateDependencies(
    IArInvoiceRepository arInvoiceRepository,
    ICustomerRepository customerRepository,
    ICurrencyRepository currencyRepository,
    IGlAccountRepository glAccountRepository,
    IItemRepository itemRepository,
    ILocationRepository locationRepository,
    ITaxRateRepository taxRateRepository
)
{
    public IArInvoiceRepository ArInvoiceRepository { get; } = arInvoiceRepository;

    public ICustomerRepository CustomerRepository { get; } = customerRepository;

    public ICurrencyRepository CurrencyRepository { get; } = currencyRepository;

    public IGlAccountRepository GlAccountRepository { get; } = glAccountRepository;

    public IItemRepository ItemRepository { get; } = itemRepository;

    public ILocationRepository LocationRepository { get; } = locationRepository;

    public ITaxRateRepository TaxRateRepository { get; } = taxRateRepository;
}
