using Microsoft.Extensions.Logging;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsReceivable.Receipts.Services;

public sealed class ArReceiptService : IArReceiptService
{
    private readonly ArReceiptCreateWorkflow createWorkflow;
    private readonly ArReceiptAllocationWorkflow allocationWorkflow;

    public ArReceiptService(
        IArReceiptRepository arReceiptRepository,
        IArReceiptAllocationRepository arReceiptAllocationRepository,
        IArInvoiceRepository arInvoiceRepository,
        ICustomerRepository customerRepository,
        IBankAccountRepository bankAccountRepository,
        ArReceiptServiceDependencies dependencies,
        ILogger<ArReceiptService> logger
    )
    {
        createWorkflow = new ArReceiptCreateWorkflow(
            arReceiptRepository,
            arReceiptAllocationRepository,
            arInvoiceRepository,
            customerRepository,
            bankAccountRepository,
            dependencies,
            logger
        );
        allocationWorkflow = new ArReceiptAllocationWorkflow(
            arReceiptRepository,
            arReceiptAllocationRepository,
            arInvoiceRepository,
            dependencies,
            logger
        );
    }

    public Task<ArReceiptCommandResultDto> CreateAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);

    public Task<ArReceiptCommandResultDto> AllocateAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken = default
    ) => allocationWorkflow.ExecuteAsync(command, cancellationToken);
}
