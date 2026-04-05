namespace OakERP.Application.AccountsPayable;

public interface IApPaymentService
{
    Task<ApPaymentCommandResultDTO> CreateAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken = default
    );

    Task<ApPaymentCommandResultDTO> AllocateAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken = default
    );
}
