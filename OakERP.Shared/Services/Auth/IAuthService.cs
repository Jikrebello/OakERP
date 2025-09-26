using OakERP.Common.DTOs.Auth;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.Services.Auth;

/// <summary>
/// Defines methods for handling user authentication, including login and registration operations.
/// </summary>
/// <remarks>This interface provides asynchronous methods for user authentication workflows.  Implementations of
/// this interface are expected to handle user credentials securely  and return appropriate results indicating the
/// success or failure of the operations.</remarks>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user based on the provided login credentials.
    /// </summary>
    /// <remarks>Ensure that the provided credentials in <see cref="LoginDTO"/> are valid and meet the
    /// expected format. The method performs authentication asynchronously and may involve network communication with an
    /// external service.</remarks>
    /// <param name="loginDTO">An object containing the user's login credentials, such as username and password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ApiResult{T}"/> object
    /// wrapping an <see cref="AuthResultDTO"/> that includes authentication details, such as tokens or user
    /// information.</returns>
    Task<ApiResult<AuthResultDTO>> LoginAsync(LoginDTO loginDTO);

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <remarks>The method performs validation on the provided registration details. If the registration is
    /// successful, the returned <see cref="AuthResultDTO"/> contains authentication tokens and user information. If the
    /// registration fails, the <see cref="ApiResult{T}"/> will indicate the failure reason.</remarks>
    /// <param name="registerDTO">An object containing the user's registration information, such as username, password, and email.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ApiResult{T}"/> object
    /// wrapping an <see cref="AuthResultDTO"/> that includes authentication details for the newly registered user.</returns>
    Task<ApiResult<AuthResultDTO>> RegisterAsync(RegisterDTO registerDTO);
}
