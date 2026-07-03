namespace OakERP.UI.ViewModels.Support;

public interface IUiOperationRunner
{
    Task RunBusyAsync(Func<Task> operation, Action<bool> setBusy, string operationName);
}
