using Microsoft.AspNetCore.Identity;

namespace OakERP.Auth;

public sealed class IdentityGateway(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager
) : IIdentityGateway
{
    public Task<ApplicationUser?> FindByEmailAsync(string email) =>
        userManager.FindByEmailAsync(email);

    public Task<IdentityResult> CreateAsync(ApplicationUser user, string password) =>
        userManager.CreateAsync(user, password);

    public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role) =>
        userManager.AddToRoleAsync(user, role);

    public Task<SignInResult> CheckPasswordSignInAsync(
        ApplicationUser user,
        string password,
        bool lockoutOnFailure
    ) => signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

    public Task<IList<string>> GetRolesAsync(ApplicationUser user) =>
        userManager.GetRolesAsync(user);
}
