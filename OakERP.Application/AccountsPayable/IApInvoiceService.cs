namespace OakERP.Application.AccountsPayable;

public interface IApInvoiceService
{
    Task<ApInvoiceCommandResultDTO> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    );
}
