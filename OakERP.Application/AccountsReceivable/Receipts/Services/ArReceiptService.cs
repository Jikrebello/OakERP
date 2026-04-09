using Microsoft.Extensions.Logging;
using OakERP.Application.Settlements.Documents;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsReceivable.Receipts.Services;

public sealed class ArReceiptService(
    IArReceiptRepository arReceiptRepository,
    IArReceiptAllocationRepository arReceiptAllocationRepository,
    IArInvoiceRepository arInvoiceRepository,
    ICustomerRepository customerRepository,
    IBankAccountRepository bankAccountRepository,
    SettlementDocumentWorkflowDependencies dependencies,
    ILogger<ArReceiptService> logger
) : IArReceiptService
{
    private readonly ArReceiptCreateWorkflow createWorkflow = new(
        arReceiptRepository,
        arReceiptAllocationRepository,
        arInvoiceRepository,
        customerRepository,
        bankAccountRepository,
        dependencies,
        logger
    );
    private readonly ArReceiptAllocationWorkflow allocationWorkflow = new(
        arReceiptRepository,
        arReceiptAllocationRepository,
        arInvoiceRepository,
        dependencies,
        logger
    );

    public Task<ArReceiptCommandResultDto> CreateAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);

    public Task<ArReceiptCommandResultDto> AllocateAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken = default
    ) => allocationWorkflow.ExecuteAsync(command, cancellationToken);
}
