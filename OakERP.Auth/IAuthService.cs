using OakERP.Common.DTOs.Auth;

namespace OakERP.Auth;

/// <summary>
/// Defines methods for user authentication and registration.
/// </summary>
/// <remarks>This interface provides functionality for registering new users and logging in existing users.
/// Implementations of this interface should handle user authentication and return appropriate results encapsulated in
/// <see cref="AuthResultDTO"/> objects.</remarks>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="dto">The registration details, including user credentials and other required information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AuthResultDTO"/>
    /// object with the authentication result, including a token if registration is successful.</returns>
    Task<AuthResultDTO> RegisterAsync(RegisterDTO dto);

    /// <summary>
    /// Authenticates a user based on the provided login credentials.
    /// </summary>
    /// <param name="dto">An object containing the user's login credentials, such as username and password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AuthResultDTO"/>
    /// object with details about the authentication result, including a token if authentication is successful.</returns>
    Task<AuthResultDTO> LoginAsync(LoginDTO dto);
}
