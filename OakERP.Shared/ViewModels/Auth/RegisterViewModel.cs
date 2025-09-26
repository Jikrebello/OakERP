using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Shared.Models.Auth;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
using OakERP.Shared.ViewModels.Base;

namespace OakERP.Shared.ViewModels.Auth;

/// <summary>
/// Represents the view model for user registration, providing functionality to handle the registration process and
/// manage related state.
/// </summary>
/// <remarks>This view model is responsible for validating the registration form, invoking the registration
/// service, and handling the resulting authentication token or error messages. It interacts with session management,
/// authentication services, and toast notifications to provide feedback to the user.</remarks>
/// <param name="session"></param>
/// <param name="authService"></param>
/// <param name="api"></param>
/// <param name="toast"></param>
internal class RegisterViewModel(
    IAuthSessionManager session,
    IAuthService authService,
    IApiClient api,
    IToastService toast
) : BaseFormViewModel<RegisterFormModel>(api)
{
    /// <summary>
    /// Registers a new user asynchronously using the provided form data.
    /// </summary>
    /// <remarks>This method validates the current form context before attempting registration. If
    /// registration is successful and a token is returned, the token is stored in the session. Otherwise, an error
    /// message is displayed.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RegisterAsync()
    {
        if (!EditContext.Validate())
            IsBusy = true;

        var result = await authService.RegisterAsync(Form);

        if (result is { Success: true } && result.Data?.Token is not null)
        {
            await session.SetTokenAsync(result.Data.Token);
        }
        else
        {
            toast.ShowError(result?.Message ?? "Register failed.");
        }

        IsBusy = false;
    }
}
