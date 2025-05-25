using System.Security.Claims;

namespace OakERP.Shared.Services.Auth;

public interface IAuthSessionManager
{
    event Action? OnUserChanged;

    Task<bool> IsAuthenticatedAsync();

    Task<ClaimsPrincipal> GetUserAsync();

    Task SetTokenAsync(string token);

    Task ClearTokenAsync();
}