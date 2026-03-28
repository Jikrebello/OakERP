using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client.Services.Auth;
using OakERP.UI.Models.Auth;

namespace OakERP.UI.ViewModels.Auth;

public class LoginViewModel
{
    private readonly IAuthSessionManager _session;
    private readonly IAuthService _authService;
    private readonly IToastService _toast;

    public LoginFormModel Form { get; } = new();

    public EditContext EditContext { get; }

    public bool IsBusy { get; private set; }

    public LoginViewModel(
        IAuthSessionManager session,
        IAuthService authService,
        IToastService toast
    )
    {
        _session = session;
        _authService = authService;
        _toast = toast;
        EditContext = new EditContext(Form);
    }

    public async Task LoginAsync()
    {
        if (!EditContext.Validate())
            IsBusy = true;

        var result = await _authService.LoginAsync(Form);

        if (result is { Success: true } && result.Data?.Token is not null)
        {
            await _session.SetTokenAsync(result.Data.Token);
        }
        else
        {
            _toast.ShowError(result?.Message ?? "Login failed.");
        }

        IsBusy = false;
    }
}
