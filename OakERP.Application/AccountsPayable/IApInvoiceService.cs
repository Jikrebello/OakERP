namespace OakERP.Application.AccountsPayable;

public interface IApInvoiceService
{
    Task<ApInvoiceCommandResultDto> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    );
}
