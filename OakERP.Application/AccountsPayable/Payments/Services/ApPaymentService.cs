using Microsoft.Extensions.Logging;
using OakERP.Application.Settlements.Documents;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsPayable.Payments.Services;

public sealed class ApPaymentService(
    IApPaymentRepository apPaymentRepository,
    IApPaymentAllocationRepository apPaymentAllocationRepository,
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    IBankAccountRepository bankAccountRepository,
    SettlementDocumentWorkflowDependencies dependencies,
    ILogger<ApPaymentService> logger
) : IApPaymentService
{
    private readonly ApPaymentCreateWorkflow createWorkflow = new(
        apPaymentRepository,
        apPaymentAllocationRepository,
        apInvoiceRepository,
        vendorRepository,
        bankAccountRepository,
        dependencies,
        logger
    );
    private readonly ApPaymentAllocationWorkflow allocationWorkflow = new(
        apPaymentRepository,
        apPaymentAllocationRepository,
        apInvoiceRepository,
        dependencies,
        logger
    );

    public Task<ApPaymentCommandResultDto> CreateAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);

    public Task<ApPaymentCommandResultDto> AllocateAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken = default
    ) => allocationWorkflow.ExecuteAsync(command, cancellationToken);
}
