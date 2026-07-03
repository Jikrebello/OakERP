using Microsoft.Extensions.Logging;

namespace OakERP.Client.Services.Errors;

internal sealed class LoggingClientErrorHandler(ILogger<LoggingClientErrorHandler> logger)
    : IClientErrorHandler
{
    public ValueTask<ClientErrorResult> HandleAsync(
        Exception exception,
        ClientErrorContext context,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogError(
            exception,
            "Unexpected client error during {Operation} for {Target}",
            context.Operation,
            context.Target ?? "(none)"
        );

        return ValueTask.FromResult(ClientErrorResult.Generic);
    }
}
