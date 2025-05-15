using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OakERP.Common.Abstractions;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
using OakERP.Shared.ViewModels.Base;

namespace OakERP.Shared.ViewModels.Auth;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ITokenStore _tokenStore;

    public EditContext EditContext { get; }

    public LoginViewModel(
        IAuthService authService,
        ITokenStore tokenStore,
        IApiClient api,
        NavigationManager nav
    )
        : base(api, nav)
    {
        _authService = authService;
        _tokenStore = tokenStore;
        EditContext = new EditContext(this);
    }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public async Task LoginAsync()
    {
        ErrorMessage = null;

        if (!EditContext.Validate())
            return;

        IsBusy = true;

        var result = await _authService.LoginAsync(Email, Password);

        if (result is { Success: true })
        {
            await _tokenStore.SaveToken(result.Token!);
            Navigation.NavigateTo("/");
            return;
        }

        ErrorMessage = result?.Message ?? "Login failed.";
        IsBusy = false;
    }
}
