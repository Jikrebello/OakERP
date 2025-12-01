using System.Security.Claims;

namespace OakERP.Common.Abstractions;

/// <summary>
/// Provides methods to retrieve information about the current user, including their identity, roles, and authentication
/// status.
/// </summary>
/// <remarks>This service is typically used to access user-specific information in applications that implement
/// authentication and authorization. It provides asynchronous methods to retrieve details such as the user's ID, email,
/// role, and authentication status.</remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Asynchronously retrieves the current user's claims principal.
    /// </summary>
    /// <remarks>The returned <see cref="ClaimsPrincipal"/> contains the claims associated with the current
    /// user,  which can be used for authorization and identity purposes.</remarks>
    /// <returns>A <see cref="ClaimsPrincipal"/> representing the current user.  If no user is authenticated, the returned value
    /// may be null or represent an unauthenticated user.</returns>
    Task<ClaimsPrincipal> GetUserAsync();

    /// <summary>
    /// Asynchronously retrieves the unique identifier of the current user.
    /// </summary>
    /// <remarks>This method is typically used to identify the currently authenticated user in the system.
    /// Ensure that the caller handles the possibility of a <see langword="null"/> return value when the user is not
    /// authenticated.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user's unique identifier as a
    /// string, or <see langword="null"/> if the user is not authenticated.</returns>
    Task<string?> GetUserIdAsync();

    /// <summary>
    /// Asynchronously retrieves the email address associated with the current user.
    /// </summary>
    /// <remarks>The returned email address may be <see langword="null"/> if the user has not provided an
    /// email or if the email is unavailable in the current context. Callers should handle this case
    /// appropriately.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains the email address as a string, or
    /// <see langword="null"/> if no email address is available.</returns>
    Task<string?> GetEmailAsync();

    /// <summary>
    /// Asynchronously retrieves the role associated with the current user.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role of the user as a string,
    /// or <see langword="null"/> if the user does not have an assigned role.</returns>
    Task<string?> GetRoleAsync();

    /// <summary>
    /// Determines whether the current user is authenticated asynchronously.
    /// </summary>
    /// <remarks>This method checks the authentication status of the current user. The result can be used  to
    /// conditionally execute operations that require authentication.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains  <see langword="true"/> if the user
    /// is authenticated; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsAuthenticatedAsync();

    Task<ClaimsPrincipal> RefreshAsync();
}
