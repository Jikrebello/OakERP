using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OakERP.Common.Abstractions;

namespace OakERP.Shared.Services.Auth;

public class CurrentUserService(ITokenStore tokenStore) : ICurrentUserService
{
    public async Task<ClaimsPrincipal> GetUserAsync()
    {
        var token = await tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var claims = jwt.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "jwt");
        return new ClaimsPrincipal(identity);
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