using Microsoft.Extensions.Logging;
using OakERP.Application.AccountsPayable.Invoices.Support;
using OakERP.Application.Common.Orchestration;

namespace OakERP.Application.AccountsPayable.Invoices.Services;

public sealed class ApInvoiceService(
    ApInvoiceCreateDependencies repositories,
    InvoiceCreateWorkflowDependencies dependencies,
    ILogger<ApInvoiceService> logger
) : IApInvoiceService
{
    private readonly ApInvoiceCreateWorkflow createWorkflow = new(
        repositories,
        dependencies,
        logger
    );

    public Task<ApInvoiceCommandResultDto> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);
}
