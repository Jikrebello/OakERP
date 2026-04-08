using Microsoft.Extensions.Logging;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsPayable.Payments.Services;

public sealed class ApPaymentService : IApPaymentService
{
    private readonly ApPaymentCreateWorkflow createWorkflow;
    private readonly ApPaymentAllocationWorkflow allocationWorkflow;

    public ApPaymentService(
        IApPaymentRepository apPaymentRepository,
        IApPaymentAllocationRepository apPaymentAllocationRepository,
        IApInvoiceRepository apInvoiceRepository,
        IVendorRepository vendorRepository,
        IBankAccountRepository bankAccountRepository,
        ApPaymentServiceDependencies dependencies,
        ILogger<ApPaymentService> logger
    )
    {
        createWorkflow = new ApPaymentCreateWorkflow(
            apPaymentRepository,
            apPaymentAllocationRepository,
            apInvoiceRepository,
            vendorRepository,
            bankAccountRepository,
            dependencies,
            logger
        );
        allocationWorkflow = new ApPaymentAllocationWorkflow(
            apPaymentRepository,
            apPaymentAllocationRepository,
            apInvoiceRepository,
            dependencies,
            logger
        );
    }

    public Task<ApPaymentCommandResultDto> CreateAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken = default
    ) => createWorkflow.ExecuteAsync(command, cancellationToken);

    public Task<ApPaymentCommandResultDto> AllocateAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken = default
    ) => allocationWorkflow.ExecuteAsync(command, cancellationToken);
}
