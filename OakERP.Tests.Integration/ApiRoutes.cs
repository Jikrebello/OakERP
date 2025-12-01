namespace OakERP.Tests.Integration;

/// <summary>
/// Provides a centralized collection of API route constants for the application.
/// </summary>
/// <remarks>This class organizes API route definitions into nested static classes, grouping related routes
/// together for better maintainability and discoverability. For example, routes related to authentication are defined
/// in the <see cref="Auth"/> nested class.</remarks>
public static class ApiRoutes
{
    public static class Auth
    {
        public const string Register = "/api/auth/register";
        public const string Login = "/api/auth/login";
    }
}
