using Microsoft.AspNetCore.Identity;
using OakERP.Domain.Entities.Users;

namespace OakERP.Auth;

/// <summary>
/// Provides the narrow subset of ASP.NET Identity operations required by <see cref="AuthService"/>.
/// </summary>
public interface IIdentityGateway
{
    Task<ApplicationUser?> FindByEmailAsync(string email);

    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);

    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);

    Task<SignInResult> CheckPasswordSignInAsync(
        ApplicationUser user,
        string password,
        bool lockoutOnFailure
    );

    Task<IList<string>> GetRolesAsync(ApplicationUser user);
}
