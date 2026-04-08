using System.Net;
using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Persistence;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repository_Interfaces.Users;

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
/// <param name="identityGateway">The auth-local gateway used to perform the required ASP.NET Identity operations.</param>
/// <param name="jwtGenerator">The <see cref="IJwtGenerator"/> instance responsible for generating JSON Web Tokens (JWTs) for authenticated
/// users.</param>
/// <param name="tenantRepository">The <see cref="ITenantRepository"/> instance used to manage tenant-specific data and operations.</param>
public class AuthService(
    IIdentityGateway identityGateway,
    IJwtGenerator jwtGenerator,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger
) : IAuthService
{
    private const string AuditActionRegistration = "UserRegistration";
    private const string AuditActionLogin = "UserLogin";
    private const string AuditOutcomeSuccess = "Success";
    private const string AuditOutcomeFailure = "Failure";
    private const string AuditReasonPasswordsDoNotMatch = "PasswordsDoNotMatch";
    private const string AuditReasonEmailAlreadyExists = "EmailAlreadyExists";
    private const string AuditReasonIdentityCreateFailed = "IdentityCreateFailed";
    private const string AuditReasonRoleAssignmentFailed = "RoleAssignmentFailed";
    private const string AuditReasonUnexpectedError = "UnexpectedError";
    private const string AuditReasonInvalidCredentials = "InvalidCredentials";
    private const string AuditReasonTenantNotFound = "TenantNotFound";
    private const string AuditReasonLicenseNotFound = "LicenseNotFound";
    private const string AuditReasonLicenseExpired = "LicenseExpired";

    private sealed record AuditContext(
        string Action,
        string Outcome,
        string Email,
        string? AuditReason = null,
        string? UserId = null,
        Guid? TenantId = null,
        string? TenantName = null
    );

    // Single mapping point from the Identity-backed ApplicationUser entity
    // into the minimal auth-local JWT input contract.
    private static JwtTokenInput MapToJwtTokenInput(ApplicationUser user) =>
        new(user.Id, user.Email!, user.TenantId);

    /// <summary>
    /// Registers a new user and creates a tenant with an associated license.
    /// </summary>
    /// <remarks>This method performs the following steps: <list type="bullet"> <item>Validates that the
    /// provided passwords match.</item> <item>Checks if the email is already registered.</item> <item>Creates a new
    /// tenant with a license valid for one year.</item> <item>Registers the user under the tenant and assigns the
    /// "TenantAdmin" role.</item> <item>Generates a JWT token for the newly registered user upon success.</item>
    /// </list> If any step fails, the operation is rolled back, and an appropriate error message is returned.</remarks>
    /// <param name="Dto">The registration details, including user information, tenant name, and password.</param>
    /// <returns>An <see cref="AuthResultDto"/> indicating the result of the registration process.  If successful, the result
    /// contains a JWT token and the user's full name.  If unsuccessful, the result contains an error message.</returns>
    public async Task<AuthResultDto> RegisterAsync(RegisterDto Dto)
    {
        var email = Dto.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        if (Dto.Password != Dto.ConfirmPassword)
        {
            LogAuditWarning(
                AuditActionRegistration,
                email,
                AuditReasonPasswordsDoNotMatch,
                tenantName: Dto.TenantName
            );
            return AuthResultDto.Fail("Passwords do not match.", HttpStatusCode.BadRequest);
        }

        var existingUser = await identityGateway.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            LogAuditWarning(
                AuditActionRegistration,
                email,
                AuditReasonEmailAlreadyExists,
                tenantName: Dto.TenantName
            );
            return AuthResultDto.Fail("Email already exists.", HttpStatusCode.Conflict);
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // Create Tenant
            var tenant = new Tenant
            {
                Name = Dto.TenantName,
                License = new License
                {
                    Key = Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                },
            };
            await tenantRepository.AddAsync(tenant);

            // Persist Tenant to get Id
            await unitOfWork.SaveChangesAsync();

            // Create User
            var user = new ApplicationUser
            {
                UserName = email,
                NormalizedUserName = normalizedEmail,
                Email = email,
                NormalizedEmail = normalizedEmail,
                FirstName = Dto.FirstName,
                LastName = Dto.LastName,
                PhoneNumber = Dto.PhoneNumber,
                EmailConfirmed = true,
                TenantId = tenant.Id,
            };

            var createUserResult = await identityGateway.CreateAsync(user, Dto.Password);
            if (!createUserResult.Succeeded)
            {
                await unitOfWork.RollbackAsync();
                LogAuditWarning(
                    AuditActionRegistration,
                    email,
                    AuditReasonIdentityCreateFailed,
                    tenantId: tenant.Id,
                    tenantName: tenant.Name
                );
                return AuthResultDto.Fail(
                    createUserResult.Errors.First().Description,
                    HttpStatusCode.BadRequest
                );
            }

            var addToRoleResult = await identityGateway.AddToRoleAsync(user, UserRoles.Admin);
            if (!addToRoleResult.Succeeded)
            {
                await unitOfWork.RollbackAsync();
                LogAuditWarning(
                    AuditActionRegistration,
                    email,
                    AuditReasonRoleAssignmentFailed,
                    userId: user.Id,
                    tenantId: tenant.Id,
                    tenantName: tenant.Name
                );
                return AuthResultDto.Fail(
                    "User created but failed to assign role.",
                    HttpStatusCode.InternalServerError
                );
            }

            // Commit Transaction
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            LogAuditInformation(
                AuditActionRegistration,
                email,
                userId: user.Id,
                tenantId: tenant.Id,
                tenantName: tenant.Name
            );

            var token = jwtGenerator.Generate(MapToJwtTokenInput(user));

            return AuthResultDto.SuccessWith(token, $"{user.FirstName} {user.LastName}");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            LogAuditError(
                ex,
                AuditActionRegistration,
                email,
                AuditReasonUnexpectedError,
                tenantName: Dto.TenantName
            );
            return AuthResultDto.Fail(
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
    /// <param name="Dto">The login data transfer object containing the user's email and password.</param>
    /// <returns>An <see cref="AuthResultDto"/> containing the authentication result. If successful, the result includes a JWT
    /// token,  the user's username, and their primary role. If authentication fails, the result contains an error
    /// message.</returns>
    public async Task<AuthResultDto> LoginAsync(LoginDto Dto)
    {
        var email = Dto.Email.Trim();
        var user = await identityGateway.FindByEmailAsync(email);

        if (user is null)
        {
            LogAuditWarning(AuditActionLogin, email, AuditReasonInvalidCredentials);
            return AuthResultDto.Fail("Invalid login credentials.", HttpStatusCode.Unauthorized);
        }

        // Check password WITHOUT signing in
        var pwdCheck = await identityGateway.CheckPasswordSignInAsync(
            user,
            Dto.Password,
            lockoutOnFailure: false // enable lockout if you want
        );

        if (!pwdCheck.Succeeded)
        {
            LogAuditWarning(
                AuditActionLogin,
                email,
                AuditReasonInvalidCredentials,
                userId: user.Id,
                tenantId: user.TenantId
            );
            return AuthResultDto.Fail("Invalid login credentials.", HttpStatusCode.Unauthorized);
        }

        // Load tenant (ideally include License in one query)
        var tenant = await tenantRepository.FindWithLicenseAsync(user.TenantId);

        if (tenant is null)
        {
            LogAuditWarning(
                AuditActionLogin,
                email,
                AuditReasonTenantNotFound,
                userId: user.Id,
                tenantId: user.TenantId
            );
            return AuthResultDto.Fail("Tenant not found.", HttpStatusCode.NotFound);
        }

        if (tenant.License is null)
        {
            LogAuditWarning(
                AuditActionLogin,
                email,
                AuditReasonLicenseNotFound,
                userId: user.Id,
                tenantId: tenant.Id,
                tenantName: tenant.Name
            );
            return AuthResultDto.Fail("License not found for tenant.", HttpStatusCode.Forbidden);
        }

        if (tenant.License.ExpiryDate is not null && tenant.License.ExpiryDate <= DateTime.UtcNow)
        {
            LogAuditWarning(
                AuditActionLogin,
                email,
                AuditReasonLicenseExpired,
                userId: user.Id,
                tenantId: tenant.Id,
                tenantName: tenant.Name
            );
            return AuthResultDto.Fail("License has expired.", HttpStatusCode.Forbidden);
        }
        var primaryRole = (await identityGateway.GetRolesAsync(user)).FirstOrDefault() ?? "User";
        var token = jwtGenerator.Generate(MapToJwtTokenInput(user));

        LogAuditInformation(
            AuditActionLogin,
            email,
            userId: user.Id,
            tenantId: tenant.Id,
            tenantName: tenant.Name
        );

        return AuthResultDto.SuccessWith(token, userName: user.UserName!, role: primaryRole);
    }

    private void LogAuditInformation(
        string action,
        string email,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    )
    {
        LogAudit(
            LogLevel.Information,
            new AuditContext(action, AuditOutcomeSuccess, email, null, userId, tenantId, tenantName)
        );
    }

    private void LogAuditWarning(
        string action,
        string email,
        string auditReason,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    )
    {
        LogAudit(
            LogLevel.Warning,
            new AuditContext(
                action,
                AuditOutcomeFailure,
                email,
                auditReason,
                userId,
                tenantId,
                tenantName
            )
        );
    }

    private void LogAuditError(
        Exception exception,
        string action,
        string email,
        string auditReason,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    )
    {
        LogAudit(
            LogLevel.Error,
            new AuditContext(
                action,
                AuditOutcomeFailure,
                email,
                auditReason,
                userId,
                tenantId,
                tenantName
            ),
            exception
        );
    }

    private void LogAudit(LogLevel level, AuditContext auditContext, Exception? exception = null)
    {
        logger.Log(
            level,
            exception,
            "Audit event {AuditAction} {AuditOutcome} for {Email}. Reason={AuditReason} UserId={UserId} TenantId={TenantId} TenantName={TenantName}",
            auditContext.Action,
            auditContext.Outcome,
            auditContext.Email,
            auditContext.AuditReason,
            auditContext.UserId,
            auditContext.TenantId,
            auditContext.TenantName
        );
    }
}
