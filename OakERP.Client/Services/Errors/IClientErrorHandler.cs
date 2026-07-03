namespace OakERP.Client.Services.Errors;

public interface IClientErrorHandler
{
    ValueTask<ClientErrorResult> HandleAsync(
        Exception exception,
        ClientErrorContext context,
        CancellationToken cancellationToken = default
    );
}
