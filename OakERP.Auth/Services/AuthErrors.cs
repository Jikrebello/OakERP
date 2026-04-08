using OakERP.Common.Errors;

namespace OakERP.Auth.Services;

public static class AuthErrors
{
    public static readonly ResultError EmailAlreadyExists =
        new("auth.email_already_exists", "Email already exists.", FailureKind.Conflict);

    public static readonly ResultError UnexpectedRegistrationFailure =
        new("auth.unexpected_registration_failure", "Unexpected error while registering user.", FailureKind.Unexpected);

    public static readonly ResultError InvalidCredentials =
        new("auth.invalid_credentials", "Invalid login credentials.", FailureKind.Unauthorized);

    public static readonly ResultError PasswordsDoNotMatch =
        new("auth.passwords_do_not_match", "Passwords do not match.", FailureKind.Validation);

    public static readonly ResultError RoleAssignmentFailed =
        new("auth.role_assignment_failed", "User created but failed to assign role.", FailureKind.Unexpected);

    public static readonly ResultError TenantNotFound =
        new("auth.tenant_not_found", "Tenant not found.", FailureKind.NotFound);

    public static readonly ResultError LicenseNotFound =
        new("auth.license_not_found", "License not found for tenant.", FailureKind.Forbidden);

    public static readonly ResultError LicenseExpired =
        new("auth.license_expired", "License has expired.", FailureKind.Forbidden);

    public static ResultError IdentityCreateFailed(string message) =>
        new("auth.identity_create_failed", message, FailureKind.Validation);
}
