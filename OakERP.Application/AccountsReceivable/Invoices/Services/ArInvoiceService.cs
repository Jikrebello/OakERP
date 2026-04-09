using Microsoft.Extensions.Logging;
using OakERP.Application.AccountsReceivable.Invoices.Support;
using OakERP.Application.Common.Orchestration;

namespace OakERP.Application.AccountsReceivable.Invoices.Services;

public sealed class ArInvoiceService(
    ArInvoiceCreateDependencies repositories,
    InvoiceCreateWorkflowDependencies dependencies,
    ILogger<ArInvoiceService> logger
) : IArInvoiceService
{
    private readonly ArInvoiceCreateWorkflow createWorkflow = new(
        repositories,
        dependencies,
        logger
    );

    public Task<ArInvoiceCommandResultDto> CreateAsync(
        CreateArInvoiceCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);
}
