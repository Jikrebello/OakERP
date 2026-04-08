using Microsoft.Extensions.Logging;

namespace OakERP.Auth.Services;

internal sealed class AuthAuditLogger(ILogger<AuthService> logger)
{
    private const string AuditActionRegistration = "UserRegistration";
    private const string AuditActionLogin = "UserLogin";
    private const string AuditOutcomeSuccess = "Success";
    private const string AuditOutcomeFailure = "Failure";

    public const string PasswordsDoNotMatch = "PasswordsDoNotMatch";
    public const string EmailAlreadyExists = "EmailAlreadyExists";
    public const string IdentityCreateFailed = "IdentityCreateFailed";
    public const string RoleAssignmentFailed = "RoleAssignmentFailed";
    public const string UnexpectedError = "UnexpectedError";
    public const string InvalidCredentials = "InvalidCredentials";
    public const string TenantNotFound = "TenantNotFound";
    public const string LicenseNotFound = "LicenseNotFound";
    public const string LicenseExpired = "LicenseExpired";

    private sealed record AuditContext(
        string Action,
        string Outcome,
        string Email,
        string? AuditReason = null,
        string? UserId = null,
        Guid? TenantId = null,
        string? TenantName = null
    );

    public void LogRegistrationSuccess(
        string email,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    ) => LogInformation(AuditActionRegistration, email, userId, tenantId, tenantName);

    public void LogRegistrationWarning(
        string email,
        string auditReason,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    ) => LogWarning(AuditActionRegistration, email, auditReason, userId, tenantId, tenantName);

    public void LogRegistrationError(
        Exception exception,
        string email,
        string auditReason,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    ) =>
        LogError(
            exception,
            AuditActionRegistration,
            email,
            auditReason,
            userId,
            tenantId,
            tenantName
        );

    public void LogLoginSuccess(
        string email,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    ) => LogInformation(AuditActionLogin, email, userId, tenantId, tenantName);

    public void LogLoginWarning(
        string email,
        string auditReason,
        string? userId = null,
        Guid? tenantId = null,
        string? tenantName = null
    ) => LogWarning(AuditActionLogin, email, auditReason, userId, tenantId, tenantName);

    private void LogInformation(
        string action,
        string email,
        string? userId,
        Guid? tenantId,
        string? tenantName
    ) =>
        Log(
            LogLevel.Information,
            new AuditContext(action, AuditOutcomeSuccess, email, null, userId, tenantId, tenantName)
        );

    private void LogWarning(
        string action,
        string email,
        string auditReason,
        string? userId,
        Guid? tenantId,
        string? tenantName
    ) =>
        Log(
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

    private void LogError(
        Exception exception,
        string action,
        string email,
        string auditReason,
        string? userId,
        Guid? tenantId,
        string? tenantName
    ) =>
        Log(
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

    private void Log(LogLevel level, AuditContext auditContext, Exception? exception = null)
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
