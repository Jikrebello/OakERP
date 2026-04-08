namespace OakERP.Application.AccountsReceivable;

public interface IArReceiptService
{
    Task<ArReceiptCommandResultDto> CreateAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken = default
    );

    Task<ArReceiptCommandResultDto> AllocateAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken = default
    );
}
