using System.Net;
using OakERP.Common.Errors;

namespace OakERP.Auth.Services;

internal static class AuthErrors
{
    public static readonly ResultError EmailAlreadyExists =
        new("Email already exists.", HttpStatusCode.Conflict);

    public static readonly ResultError UnexpectedRegistrationFailure =
        new(
            "An unexpected error occurred during registration.",
            HttpStatusCode.InternalServerError
        );

    public static readonly ResultError InvalidCredentials =
        new("Invalid login credentials.", HttpStatusCode.Unauthorized);

    public static readonly ResultError PasswordsDoNotMatch =
        new("Passwords do not match.", HttpStatusCode.BadRequest);

    public static readonly ResultError RoleAssignmentFailed =
        new("User created but failed to assign role.", HttpStatusCode.InternalServerError);

    public static readonly ResultError TenantNotFound =
        new("Tenant not found.", HttpStatusCode.NotFound);

    public static readonly ResultError LicenseNotFound =
        new("License not found for tenant.", HttpStatusCode.Forbidden);

    public static readonly ResultError LicenseExpired =
        new("License has expired.", HttpStatusCode.Forbidden);
}
