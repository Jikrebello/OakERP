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

    public static class ApInvoices
    {
        public const string Create = "/api/ap-invoices";
    }

    public static class ApPayments
    {
        public const string Create = "/api/ap-payments";

        public static string Allocate(Guid paymentId) =>
            $"/api/ap-payments/{paymentId}/allocations";
    }

    public static class ArReceipts
    {
        public const string Create = "/api/ar-receipts";

        public static string Allocate(Guid receiptId) =>
            $"/api/ar-receipts/{receiptId}/allocations";
    }
}
