using OakERP.Application.Interfaces.Persistence;
using OakERP.Auth.Identity;
using OakERP.Auth.Internal.Support;
using OakERP.Auth.Jwt;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Persistence;
using OakERP.Common.Time;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.RepositoryInterfaces.Users;

namespace OakERP.Auth.Services;

internal sealed class AuthRegistrationWorkflow(
    IIdentityGateway identityGateway,
    IJwtGenerator jwtGenerator,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    AuthAuditLogger auditLogger,
    IdentityFailureMapper identityFailureMapper,
    AuthTransactionRunner transactionRunner
)
{
    public async Task<AuthResultDto> RegisterAsync(RegisterDto Dto)
    {
        var email = Dto.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        AuthResultDto? validationFailure = ValidateRegistrationInput(Dto, email);
        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var existingUser = await identityGateway.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            auditLogger.LogRegistrationWarning(
                email,
                AuthAuditLogger.EmailAlreadyExists,
                tenantName: Dto.TenantName
            );
            return AuthResultDto.Fail(AuthErrors.EmailAlreadyExists);
        }

        try
        {
            return await transactionRunner.ExecuteAsync(
                async () =>
                {
                    Tenant tenant = CreateTenant(Dto.TenantName);
                    await tenantRepository.AddAsync(tenant);
                    await unitOfWork.SaveChangesAsync();

                    ApplicationUser user = CreateUser(Dto, email, normalizedEmail, tenant.Id);
                    AuthResultDto? createUserFailure = await TryCreateIdentityUserAsync(
                        user,
                        Dto.Password,
                        email,
                        tenant
                    );
                    if (createUserFailure is not null)
                    {
                        return createUserFailure;
                    }

                    AuthResultDto? roleFailure = await TryAssignAdminRoleAsync(user, email, tenant);
                    if (roleFailure is not null)
                    {
                        return roleFailure;
                    }

                    return BuildRegistrationSuccess(user, email, tenant);
                }
            );
        }
        catch (Exception ex)
        {
            auditLogger.LogRegistrationError(
                ex,
                email,
                AuthAuditLogger.UnexpectedError,
                tenantName: Dto.TenantName
            );
            return AuthResultDto.Fail(AuthErrors.UnexpectedRegistrationFailure);
        }
    }

    private AuthResultDto? ValidateRegistrationInput(RegisterDto Dto, string email)
    {
        if (Dto.Password == Dto.ConfirmPassword)
        {
            return null;
        }

        auditLogger.LogRegistrationWarning(
            email,
            AuthAuditLogger.PasswordsDoNotMatch,
            tenantName: Dto.TenantName
        );
        return AuthResultDto.Fail(AuthErrors.PasswordsDoNotMatch);
    }

    private Tenant CreateTenant(string tenantName)
    {
        var now = clock.UtcNow.UtcDateTime;
        return new Tenant
        {
            Name = tenantName,
            License = new License
            {
                Key = Guid.NewGuid().ToString("N"),
                CreatedAt = now,
                ExpiryDate = now.AddYears(1),
            },
        };
    }

    private static ApplicationUser CreateUser(
        RegisterDto Dto,
        string email,
        string normalizedEmail,
        Guid tenantId
    ) =>
        new()
        {
            UserName = email,
            NormalizedUserName = normalizedEmail,
            Email = email,
            NormalizedEmail = normalizedEmail,
            FirstName = Dto.FirstName,
            LastName = Dto.LastName,
            PhoneNumber = Dto.PhoneNumber,
            EmailConfirmed = true,
            TenantId = tenantId,
        };

    private async Task<AuthResultDto?> TryCreateIdentityUserAsync(
        ApplicationUser user,
        string password,
        string email,
        Tenant tenant
    )
    {
        var createUserResult = await identityGateway.CreateAsync(user, password);
        if (createUserResult.Succeeded)
        {
            return null;
        }

        auditLogger.LogRegistrationWarning(
            email,
            AuthAuditLogger.IdentityCreateFailed,
            tenantId: tenant.Id,
            tenantName: tenant.Name
        );
        return identityFailureMapper.MapCreateFailure(createUserResult);
    }

    private async Task<AuthResultDto?> TryAssignAdminRoleAsync(
        ApplicationUser user,
        string email,
        Tenant tenant
    )
    {
        var addToRoleResult = await identityGateway.AddToRoleAsync(user, UserRoles.Admin);
        if (addToRoleResult.Succeeded)
        {
            return null;
        }

        auditLogger.LogRegistrationWarning(
            email,
            AuthAuditLogger.RoleAssignmentFailed,
            userId: user.Id,
            tenantId: tenant.Id,
            tenantName: tenant.Name
        );
        return AuthResultDto.Fail(AuthErrors.RoleAssignmentFailed);
    }

    private AuthResultDto BuildRegistrationSuccess(
        ApplicationUser user,
        string email,
        Tenant tenant
    )
    {
        auditLogger.LogRegistrationSuccess(
            email,
            userId: user.Id,
            tenantId: tenant.Id,
            tenantName: tenant.Name
        );

        string token = jwtGenerator.Generate(new JwtTokenInput(user.Id, user.Email!, user.TenantId));
        return AuthResultDto.SuccessWith(token, $"{user.FirstName} {user.LastName}");
    }
}
