using Microsoft.Extensions.Logging;
using OakERP.Client.Services.Errors;

namespace OakERP.UI.Errors;

public sealed class ToastClientErrorHandler(
    ILogger<ToastClientErrorHandler> logger,
    IUiErrorNotifier notifier
) : IClientErrorHandler
{
    public ValueTask<ClientErrorResult> HandleAsync(
        Exception exception,
        ClientErrorContext context,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogError(
            exception,
            "Unexpected frontend error during {Operation} for {Target}",
            context.Operation,
            context.Target ?? "(none)"
        );
        notifier.ShowUnexpectedError();

        return ValueTask.FromResult(ClientErrorResult.Generic);
    }
}
