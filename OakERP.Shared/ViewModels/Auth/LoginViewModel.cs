using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Common.Abstractions;
using OakERP.Shared.Models.Auth;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
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
public class LoginViewModel(
    IAuthService authService,
    ITokenStore tokenStore,
    IApiClient api,
    NavigationManager nav,
    IToastService toast
) : BaseFormViewModel<LoginFormModel>(api, nav)
{
    /// <summary>
    /// Attempts to log in the user with the provided credentials.
    /// </summary>
    /// <remarks>This method validates the input form, performs the login operation, and handles the result.
    /// If the login is successful, the authentication token is saved, and the user is redirected to the home page.  If
    /// the login fails, an error message is set.</remarks>
    /// <returns></returns>
    public async Task LoginAsync()
    {
        ErrorMessage = null;

        if (!EditContext.Validate())
            return;

        IsBusy = true;

        var result = await authService.LoginAsync(Form);

        if (result is { Success: true })
        {
            await tokenStore.SaveTokenAsync(result.Token!);
            Navigation.NavigateTo("/");
            return;
        }

        toast.ShowError(result?.Message ?? "Login failed.");
        IsBusy = false;
    }
}