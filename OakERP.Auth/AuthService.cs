using System.Net;
using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Persistence;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repository_Interfaces.Users;

namespace OakERP.Auth;

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
