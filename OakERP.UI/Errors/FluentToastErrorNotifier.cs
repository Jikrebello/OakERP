using Microsoft.FluentUI.AspNetCore.Components;

namespace OakERP.UI.Errors;

internal sealed class FluentToastErrorNotifier(IToastService toastService) : IUiErrorNotifier
{
    public void ShowUnexpectedError()
    {
        toastService.ShowError("An unexpected error occurred.");
    }
}
