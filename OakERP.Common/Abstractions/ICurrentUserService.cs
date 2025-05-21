using System.Security.Claims;

namespace OakERP.Common.Abstractions;

public interface ICurrentUserService
{
    Task<ClaimsPrincipal> GetUserAsync();

    Task<string?> GetUserIdAsync();

    Task<string?> GetEmailAsync();

    Task<string?> GetRoleAsync();

    Task<bool> IsAuthenticatedAsync();
}