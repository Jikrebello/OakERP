using System.Security.Claims;
using OakERP.Common.Abstractions;

namespace OakERP.Client.Services.Auth;

public class AuthSessionManager(ITokenStore tokenStore, ICurrentUserService currentUser)
    : IAuthSessionManager
{
    public event Action? OnUserChanged;

    public async Task<ClaimsPrincipal> GetUserAsync() => await currentUser.GetUserAsync();

    public async Task<bool> IsAuthenticatedAsync() => await currentUser.IsAuthenticatedAsync();

    public async Task SetTokenAsync(string token)
    {
        await tokenStore.SaveTokenAsync(token);
        await currentUser.RefreshAsync();
        OnUserChanged?.Invoke();
    }

    public async Task ClearTokenAsync()
    {
        await tokenStore.DeleteTokenAsync();
        await currentUser.RefreshAsync();
        OnUserChanged?.Invoke();
    }
}
