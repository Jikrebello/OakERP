namespace OakERP.Application.AccountsReceivable;

public interface IArReceiptService
{
    Task<ArReceiptCommandResultDTO> CreateAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken = default
    );

    Task<ArReceiptCommandResultDTO> AllocateAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken = default
    );
}
