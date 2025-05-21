using Microsoft.AspNetCore.Identity;
using OakERP.Common.DTOs.Auth;
using OakERP.Domain.Entities;
using OakERP.Domain.Repositories;

namespace OakERP.Auth;

/// <summary>
/// Provides authentication and authorization services, including user registration, login,  and token generation. This
/// service integrates with ASP.NET Identity and supports tenant-based licensing.
/// </summary>
/// <remarks>The <see cref="AuthService"/> class is designed to handle user authentication and authorization
/// workflows, such as registering new users, validating login credentials, and generating JWT tokens.  It also ensures
/// that users are associated with tenants and validates tenant licenses during login.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthService"/> class, providing services for user authentication
/// and token generation.
/// </remarks>
/// <param name="userManager">The <see cref="UserManager{TUser}"/> instance used to manage user accounts and perform user-related operations.</param>
/// <param name="signInManager">The <see cref="SignInManager{TUser}"/> instance used to handle user sign-in operations.</param>
/// <param name="jwtGenerator">The <see cref="IJwtGenerator"/> instance responsible for generating JSON Web Tokens (JWTs) for authenticated
/// users.</param>
/// <param name="tenantRepository">The <see cref="ITenantRepository"/> instance used to manage tenant-specific data and operations.</param>
public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtGenerator jwtGenerator,
    ITenantRepository tenantRepository
) : IAuthService
{
    /// <summary>
    /// Registers a new user and creates a tenant with an associated license.
    /// </summary>
    /// <remarks>This method performs the following steps: <list type="bullet"> <item>Validates that the
    /// provided password matches the confirmation password.</item> <item>Checks if a user with the specified email
    /// already exists.</item> <item>Creates a new tenant with a license valid for one year.</item> <item>Registers the
    /// user under the newly created tenant.</item> <item>Generates a JWT token for the registered user upon successful
    /// registration.</item> </list></remarks>
    /// <param name="dto">The registration details, including user credentials, tenant name, and password confirmation.</param>
    /// <returns>An <see cref="AuthResultDTO"/> indicating the result of the registration process.  If successful, the result
    /// contains a JWT token and the username of the newly registered user.  If unsuccessful, the result contains an
    /// error message describing the failure.</returns>
    public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return AuthResultDTO.Fail("Passwords do not match.");

        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return AuthResultDTO.Fail("Email already exists.");

        var tenant = new Tenant
        {
            Name = dto.TenantName,
            License = new License
            {
                Key = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
            },
        };

        await tenantRepository.CreateAsync(tenant);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            TenantId = tenant.Id,
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return AuthResultDTO.Fail(result.Errors.First().Description);

        var token = jwtGenerator.Generate(user);
        return AuthResultDTO.SuccessWith(token, userName: user.UserName);
    }

    /// <summary>
    /// Authenticates a user based on the provided login credentials and returns an authentication result.
    /// </summary>
    /// <remarks>This method performs several validation checks during the login process: <list type="bullet">
    /// <item><description>Verifies the user's email and password using the sign-in manager.</description></item>
    /// <item><description>Ensures the user exists in the system.</description></item> <item><description>Validates the
    /// tenant associated with the user, including the presence and validity of a license.</description></item> </list>
    /// If any of these checks fail, the method returns a failure result with an appropriate error message.</remarks>
    /// <param name="dto">The login data transfer object containing the user's email and password.</param>
    /// <returns>An <see cref="AuthResultDTO"/> containing the authentication result. If successful, the result includes a JWT
    /// token,  the user's username, and their primary role. If authentication fails, the result contains an error
    /// message.</returns>
    public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
    {
        var result = await signInManager.PasswordSignInAsync(
            dto.Email,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: false
        );

        if (!result.Succeeded)
            return AuthResultDTO.Fail("Invalid login credentials.");

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return AuthResultDTO.Fail("User not found.");

        var tenant = await tenantRepository.GetByIdAsync(user.TenantId);
        if (tenant is null)
            return AuthResultDTO.Fail("Tenant not found.");

        if (tenant.License is null)
            return AuthResultDTO.Fail("License not found for tenant.");

        if (tenant.License.ExpiryDate is not null && tenant.License.ExpiryDate < DateTime.UtcNow)
            return AuthResultDTO.Fail("License has expired.");

        var token = jwtGenerator.Generate(user);

        var primaryRole = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

        return AuthResultDTO.SuccessWith(token, userName: user.UserName, role: primaryRole);
    }
}