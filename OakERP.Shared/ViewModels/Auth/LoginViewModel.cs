using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client.Services.Api;
using OakERP.Client.Services.Auth;
using OakERP.Shared.Models.Auth;
using OakERP.Shared.ViewModels.Base;

namespace OakERP.Shared.ViewModels.Auth;

/// <summary>
/// Represents the view model for the login functionality, providing methods and state management for user
/// authentication.
/// </summary>
/// <remarks>This view model is responsible for handling the login process, including form validation, interaction
/// with the authentication service, and navigation upon successful login. It also manages error messages and busy state
/// during the login operation.</remarks>
/// <param name="authService"></param>
/// <param name="tokenStore"></param>
/// <param name="api"></param>
/// <param name="nav"></param>
internal class LoginViewModel(
    IAuthSessionManager session,
    IAuthService authService,
    IApiClient api,
    IToastService toast
) : BaseFormViewModel<LoginFormModel>(api)
{
    /// <summary>
    /// Attempts to log in the user using the provided form data.
    /// </summary>
    /// <remarks>This method validates the form data before initiating the login process. If the login is
    /// successful, the authentication token is stored in the session. If the login fails, an error message is displayed
    /// to the user.</remarks>
    /// <returns>A task that represents the asynchronous login operation.</returns>
    public async Task LoginAsync()
    {
        if (!EditContext.Validate())
            IsBusy = true;

        var result = await authService.LoginAsync(Form);

        if (result is { Success: true } && result.Data?.Token is not null)
        {
            await session.SetTokenAsync(result.Data.Token);
        }
        else
        {
            toast.ShowError(result?.Message ?? "Login failed.");
        }

        IsBusy = false;
    }
}
