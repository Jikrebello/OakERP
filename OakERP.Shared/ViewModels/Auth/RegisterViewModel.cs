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
    public async Task RegisterAsync()
    {
        if (!EditContext.Validate())
            IsBusy = true;

        var result = await authService.RegisterAsync(Form);

        if (result is { Success: true })
        {
            await session.SetTokenAsync(result.Token!);
        }
        else
        {
            toast.ShowError(result?.Message ?? "Register failed.");
        }

        IsBusy = false;
    }
}