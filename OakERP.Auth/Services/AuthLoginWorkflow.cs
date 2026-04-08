using OakERP.Auth.Identity;
using OakERP.Auth.Jwt;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Time;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.RepositoryInterfaces.Users;

namespace OakERP.Auth.Services;

internal sealed class AuthLoginWorkflow(
    IIdentityGateway identityGateway,
    IJwtGenerator jwtGenerator,
    ITenantRepository tenantRepository,
    IClock clock,
    AuthAuditLogger auditLogger
)
{
    public async Task<AuthResultDto> LoginAsync(LoginDto Dto)
    {
        var email = Dto.Email.Trim();
        ApplicationUser? user = await identityGateway.FindByEmailAsync(email);

        if (user is null)
        {
            auditLogger.LogLoginWarning(email, AuthAuditLogger.InvalidCredentials);
            return AuthResultDto.Fail(AuthErrors.InvalidCredentials);
        }

        AuthResultDto? credentialFailure = await ValidateLoginCredentialsAsync(user, Dto, email);
        if (credentialFailure is not null)
        {
            return credentialFailure;
        }

        var (tenant, tenantFailure) = await GetValidatedTenantAsync(user, email);
        if (tenantFailure is not null)
        {
            return tenantFailure;
        }

        return await BuildLoginSuccessAsync(user, tenant!, email);
    }

    private async Task<AuthResultDto?> ValidateLoginCredentialsAsync(
        ApplicationUser user,
        LoginDto Dto,
        string email
    )
    {
        var pwdCheck = await identityGateway.CheckPasswordSignInAsync(
            user,
            Dto.Password,
            lockoutOnFailure: false
        );

        if (pwdCheck.Succeeded)
        {
            return null;
        }

        auditLogger.LogLoginWarning(
            email,
            AuthAuditLogger.InvalidCredentials,
            userId: user.Id,
            tenantId: user.TenantId
        );
        return AuthResultDto.Fail(AuthErrors.InvalidCredentials);
    }

    private async Task<(Tenant? tenant, AuthResultDto? failure)> GetValidatedTenantAsync(
        ApplicationUser user,
        string email
    )
    {
        Tenant? tenant = await tenantRepository.FindWithLicenseAsync(user.TenantId);

        if (tenant is null)
        {
            auditLogger.LogLoginWarning(
                email,
                AuthAuditLogger.TenantNotFound,
                userId: user.Id,
                tenantId: user.TenantId
            );
            return (null, AuthResultDto.Fail(AuthErrors.TenantNotFound));
        }

        if (tenant.License is null)
        {
            auditLogger.LogLoginWarning(
                email,
                AuthAuditLogger.LicenseNotFound,
                userId: user.Id,
                tenantId: tenant.Id,
                tenantName: tenant.Name
            );
            return (null, AuthResultDto.Fail(AuthErrors.LicenseNotFound));
        }

        if (
            tenant.License.ExpiryDate is not null
            && tenant.License.ExpiryDate <= clock.UtcNow.UtcDateTime
        )
        {
            auditLogger.LogLoginWarning(
                email,
                AuthAuditLogger.LicenseExpired,
                userId: user.Id,
                tenantId: tenant.Id,
                tenantName: tenant.Name
            );
            return (null, AuthResultDto.Fail(AuthErrors.LicenseExpired));
        }

        return (tenant, null);
    }

    private async Task<AuthResultDto> BuildLoginSuccessAsync(
        ApplicationUser user,
        Tenant tenant,
        string email
    )
    {
        string primaryRole = (await identityGateway.GetRolesAsync(user)).FirstOrDefault() ?? "User";
        string token = jwtGenerator.Generate(
            new JwtTokenInput(user.Id, user.Email!, user.TenantId)
        );

        auditLogger.LogLoginSuccess(
            email,
            userId: user.Id,
            tenantId: tenant.Id,
            tenantName: tenant.Name
        );

        return AuthResultDto.SuccessWith(token, userName: user.UserName!, role: primaryRole);
    }
}
