using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.DTOs.Auth;
using OakERP.Common.Persistence;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repositories.Users;

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
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger
) : IAuthService
{
    /// <summary>
    /// Registers a new user and creates a tenant with an associated license.
    /// </summary>
    /// <remarks>This method performs the following steps: <list type="bullet"> <item>Validates that the
    /// provided passwords match.</item> <item>Checks if the email is already registered.</item> <item>Creates a new
    /// tenant with a license valid for one year.</item> <item>Registers the user under the tenant and assigns the
    /// "TenantAdmin" role.</item> <item>Generates a JWT token for the newly registered user upon success.</item>
    /// </list> If any step fails, the operation is rolled back, and an appropriate error message is returned.</remarks>
    /// <param name="dto">The registration details, including user information, tenant name, and password.</param>
    /// <returns>An <see cref="AuthResultDTO"/> indicating the result of the registration process.  If successful, the result
    /// contains a JWT token and the user's full name.  If unsuccessful, the result contains an error message.</returns>
    public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
    {
        var email = dto.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        if (dto.Password != dto.ConfirmPassword)
            return AuthResultDTO.Fail("Passwords do not match.", HttpStatusCode.BadRequest);

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return AuthResultDTO.Fail("Email already exists.", HttpStatusCode.Conflict);

        await unitOfWork.BeginTransactionAsync();
        try
        {
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
                UserName = email,
                NormalizedUserName = normalizedEmail,
                Email = email,
                NormalizedEmail = normalizedEmail,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true,
                TenantId = tenant.Id,
            };

            var createUserResult = await userManager.CreateAsync(user, dto.Password);
            if (!createUserResult.Succeeded)
            {
                await unitOfWork.RollbackAsync();
                return AuthResultDTO.Fail(
                    createUserResult.Errors.First().Description,
                    HttpStatusCode.BadRequest
                );
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, UserRoles.Admin);
            if (!addToRoleResult.Succeeded)
            {
                await unitOfWork.RollbackAsync();
                return AuthResultDTO.Fail(
                    "User created but failed to assign role.",
                    HttpStatusCode.InternalServerError
                );
            }

            await unitOfWork.CommitAsync();

            logger.LogInformation(
                "New user registered: {Email} under tenant {TenantName}",
                user.Email,
                tenant.Name
            );

            var token = jwtGenerator.Generate(user);
            return AuthResultDTO.SuccessWith(token, $"{user.FirstName} {user.LastName}");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Registration failed for email: {Email}", dto.Email);
            return AuthResultDTO.Fail(
                "An unexpected error occurred during registration.",
                HttpStatusCode.InternalServerError
            );
        }
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
            return AuthResultDTO.Fail("Invalid login credentials.", HttpStatusCode.Unauthorized);

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return AuthResultDTO.Fail("User not found.", HttpStatusCode.NotFound);

        var tenant = await tenantRepository.GetByIdAsync(user.TenantId);
        if (tenant is null)
            return AuthResultDTO.Fail("Tenant not found.", HttpStatusCode.NotFound);

        if (tenant.License is null)
            return AuthResultDTO.Fail("License not found for tenant.", HttpStatusCode.NotFound);

        if (tenant.License.ExpiryDate is not null && tenant.License.ExpiryDate < DateTime.UtcNow)
            return AuthResultDTO.Fail("License has expired.", HttpStatusCode.Forbidden);

        var token = jwtGenerator.Generate(user);

        var primaryRole = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

        return AuthResultDTO.SuccessWith(token, userName: user.UserName, role: primaryRole);
    }
}
