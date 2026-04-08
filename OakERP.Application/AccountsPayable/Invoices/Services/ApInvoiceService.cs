using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;

namespace OakERP.Application.AccountsPayable.Invoices.Services;

public sealed class ApInvoiceService : IApInvoiceService
{
    private readonly ApInvoiceCreateWorkflow createWorkflow;

    public ApInvoiceService(
        IApInvoiceRepository apInvoiceRepository,
        IVendorRepository vendorRepository,
        ICurrencyRepository currencyRepository,
        IGlAccountRepository glAccountRepository,
        IUnitOfWork unitOfWork,
        IPersistenceFailureClassifier persistenceFailureClassifier,
        IClock clock,
        ILogger<ApInvoiceService> logger
    )
    {
        createWorkflow = new ApInvoiceCreateWorkflow(
            apInvoiceRepository,
            vendorRepository,
            currencyRepository,
            glAccountRepository,
            unitOfWork,
            persistenceFailureClassifier,
            clock,
            logger
        );
    }

    public Task<ApInvoiceCommandResultDto> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);
}
