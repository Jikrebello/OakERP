using OakERP.Common.Dtos.Auth;

namespace OakERP.Auth;

/// <summary>
/// Defines methods for user authentication and registration.
/// </summary>
/// <remarks>This interface provides functionality for registering new users and logging in existing users.
/// Implementations of this interface should handle user authentication and return appropriate results encapsulated in
/// <see cref="AuthResultDto"/> objects.</remarks>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="Dto">The registration details, including user credentials and other required information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AuthResultDto"/>
    /// object with the authentication result, including a token if registration is successful.</returns>
    Task<AuthResultDto> RegisterAsync(RegisterDto Dto);

    /// <summary>
    /// Authenticates a user based on the provided login credentials.
    /// </summary>
    /// <param name="Dto">An object containing the user's login credentials, such as username and password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AuthResultDto"/>
    /// object with details about the authentication result, including a token if authentication is successful.</returns>
    Task<AuthResultDto> LoginAsync(LoginDto Dto);
}
