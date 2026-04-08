namespace OakERP.Application.AccountsReceivable.Invoices.Services;

public interface IArInvoiceService
{
    Task<ArInvoiceCommandResultDto> CreateAsync(
        CreateArInvoiceCommand command,
        CancellationToken cancellationToken = default
    );
}
