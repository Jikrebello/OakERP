using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Auth.Identity;
using OakERP.Auth.Jwt;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Time;
using OakERP.Domain.RepositoryInterfaces.Users;

namespace OakERP.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly AuthRegistrationWorkflow registrationWorkflow;
    private readonly AuthLoginWorkflow loginWorkflow;

    public AuthService(
        IIdentityGateway identityGateway,
        IJwtGenerator jwtGenerator,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<AuthService> logger
    )
    {
        var auditLogger = new AuthAuditLogger(logger);
        var identityFailureMapper = new IdentityFailureMapper();

        registrationWorkflow = new AuthRegistrationWorkflow(
            identityGateway,
            jwtGenerator,
            tenantRepository,
            unitOfWork,
            clock,
            auditLogger,
            identityFailureMapper
        );
        loginWorkflow = new AuthLoginWorkflow(
            identityGateway,
            jwtGenerator,
            tenantRepository,
            clock,
            auditLogger
        );
    }

    public Task<AuthResultDto> RegisterAsync(RegisterDto Dto) => registrationWorkflow.RegisterAsync(Dto);

    public Task<AuthResultDto> LoginAsync(LoginDto Dto) => loginWorkflow.LoginAsync(Dto);
}
