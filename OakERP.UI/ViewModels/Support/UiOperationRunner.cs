using OakERP.Client.Services.Errors;

namespace OakERP.UI.ViewModels.Support;

public sealed class UiOperationRunner(IClientErrorHandler errorHandler) : IUiOperationRunner
{
    public async Task RunBusyAsync(Func<Task> operation, Action<bool> setBusy, string operationName)
    {
        setBusy(true);

        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            await errorHandler.HandleAsync(ex, new ClientErrorContext(operationName));
        }
        finally
        {
            setBusy(false);
        }
    }
}
