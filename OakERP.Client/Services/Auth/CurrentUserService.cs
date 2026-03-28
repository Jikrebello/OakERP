using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OakERP.Common.Abstractions;

namespace OakERP.Client.Services.Auth;

public class CurrentUserService(ITokenStore tokenStore) : ICurrentUserService
{
    private ClaimsPrincipal? _cachedUser;

    public async Task<ClaimsPrincipal> GetUserAsync()
    {
        if (_cachedUser is not null)
            return _cachedUser;

        return await RefreshAsync();
    }

    public async Task<ClaimsPrincipal> RefreshAsync()
    {
        var token = await tokenStore.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            return _cachedUser;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var claims = jwt.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "jwt");
        _cachedUser = new ClaimsPrincipal(identity);

        return _cachedUser;
    }

    public async Task<string?> GetUserIdAsync() =>
        (await GetUserAsync()).FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public async Task<string?> GetEmailAsync() =>
        (await GetUserAsync()).FindFirst(ClaimTypes.Email)?.Value;

    public async Task<string?> GetRoleAsync() =>
        (await GetUserAsync()).FindFirst(ClaimTypes.Role)?.Value;

    public async Task<bool> IsAuthenticatedAsync() =>
        (await GetUserAsync()).Identity?.IsAuthenticated == true;
}
