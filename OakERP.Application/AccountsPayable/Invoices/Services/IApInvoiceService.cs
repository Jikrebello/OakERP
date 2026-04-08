namespace OakERP.Application.AccountsPayable.Invoices.Services;

public interface IApInvoiceService
{
    Task<ApInvoiceCommandResultDto> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    );
}
