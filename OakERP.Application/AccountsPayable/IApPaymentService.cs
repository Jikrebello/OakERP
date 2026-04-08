namespace OakERP.Application.AccountsPayable;

public interface IApPaymentService
{
    Task<ApPaymentCommandResultDto> CreateAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken = default
    );

    Task<ApPaymentCommandResultDto> AllocateAsync(
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken = default
    );
}
