using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Shared.Models.Auth;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
using OakERP.Shared.ViewModels.Base;

namespace OakERP.Shared.ViewModels.Auth;

internal class RegisterViewModel(
    IAuthService authService,
    IApiClient api,
    NavigationManager navigation,
    IToastService toast
) : BaseFormViewModel<RegisterFormModel>(api, navigation)
{
    public async Task RegisterAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (!EditContext.Validate())
            return;

        IsBusy = true;

        var result = await authService.RegisterAsync(Form);

        if (result.Success)
        {
            toast.ShowSuccess(title: result?.Message);
            SuccessMessage = "Account created successfully!";
            Navigation.NavigateTo("/auth/login");
        }
        else
        {
            toast.ShowError(result?.Message ?? "Login failed.");
        }

        IsBusy = false;
    }
}