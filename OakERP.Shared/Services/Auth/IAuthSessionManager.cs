using System.Security.Claims;

namespace OakERP.Shared.Services.Auth;

/// <summary>
/// Defines methods and events for managing authentication sessions, including user state and token handling.
/// </summary>
/// <remarks>This interface provides functionality to check authentication status, retrieve the current user,  and
/// manage authentication tokens. It also includes an event to notify when the user state changes.</remarks>
public interface IAuthSessionManager
{
    event Action? OnUserChanged;

    /// <summary>
    /// Asynchronously determines whether the current user is authenticated.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user is
    /// authenticated; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Asynchronously retrieves the current user's claims principal.
    /// </summary>
    /// <returns>A <see cref="ClaimsPrincipal"/> representing the current user. Returns <see langword="null"/> if no user is
    /// authenticated.</returns>
    Task<ClaimsPrincipal> GetUserAsync();

    /// <summary>
    /// Asynchronously sets the authentication token to be used for subsequent operations.
    /// </summary>
    /// <remarks>The provided token will be used to authenticate future requests. Ensure the token is valid
    /// and has not expired.</remarks>
    /// <param name="token">The authentication token to set. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetTokenAsync(string token);

    /// <summary>
    /// Clears the stored authentication token asynchronously.
    /// </summary>
    /// <remarks>This method removes any previously stored authentication token, ensuring that subsequent
    /// operations requiring authentication will need a new token. It is typically used to log out a user or reset the
    /// authentication state.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ClearTokenAsync();
}
