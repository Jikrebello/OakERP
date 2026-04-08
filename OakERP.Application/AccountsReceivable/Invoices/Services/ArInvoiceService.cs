using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.Inventory;

namespace OakERP.Application.AccountsReceivable.Invoices.Services;

public sealed class ArInvoiceService : IArInvoiceService
{
    private readonly ArInvoiceCreateWorkflow createWorkflow;

    public ArInvoiceService(
        IArInvoiceRepository arInvoiceRepository,
        ICustomerRepository customerRepository,
        ICurrencyRepository currencyRepository,
        IGlAccountRepository glAccountRepository,
        IItemRepository itemRepository,
        ILocationRepository locationRepository,
        ITaxRateRepository taxRateRepository,
        IUnitOfWork unitOfWork,
        IPersistenceFailureClassifier persistenceFailureClassifier,
        IClock clock,
        ILogger<ArInvoiceService> logger
    )
    {
        createWorkflow = new ArInvoiceCreateWorkflow(
            arInvoiceRepository,
            customerRepository,
            currencyRepository,
            glAccountRepository,
            itemRepository,
            locationRepository,
            taxRateRepository,
            unitOfWork,
            persistenceFailureClassifier,
            clock,
            logger
        );
    }

    public Task<ArInvoiceCommandResultDto> CreateAsync(
        CreateArInvoiceCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);
}
