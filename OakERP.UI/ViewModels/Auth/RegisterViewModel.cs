using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client.Services.Auth;
using OakERP.UI.Models.Auth;
using OakERP.UI.ViewModels.Support;

namespace OakERP.UI.ViewModels.Auth;

public class RegisterViewModel
{
    private readonly IAuthSessionManager _session;
    private readonly IAuthService _authService;
    private readonly IToastService _toast;
    private readonly IUiOperationRunner _operationRunner;

    public RegisterFormModel Form { get; } = new();

    public EditContext EditContext { get; }

    public bool IsBusy { get; private set; }

    public RegisterViewModel(
        IAuthSessionManager session,
        IAuthService authService,
        IToastService toast,
        IUiOperationRunner operationRunner
    )
    {
        _session = session;
        _authService = authService;
        _toast = toast;
        _operationRunner = operationRunner;
        EditContext = new EditContext(Form);
    }

    public async Task RegisterAsync()
    {
        if (!EditContext.Validate())
            return;

        await _operationRunner.RunBusyAsync(SubmitRegisterAsync, SetBusy, "Register");
    }

    private async Task SubmitRegisterAsync()
    {
        var result = await _authService.RegisterAsync(Form);

        if (result is { Success: true } && result.Data?.Token is not null)
        {
            await _session.SetTokenAsync(result.Data.Token);
        }
        else
        {
            _toast.ShowError(result?.Message ?? "Register failed.");
        }
    }

    private void SetBusy(bool isBusy) => IsBusy = isBusy;
}
