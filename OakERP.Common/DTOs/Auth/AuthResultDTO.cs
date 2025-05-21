using OakERP.Common.DTOs.Base;

namespace OakERP.Common.DTOs.Auth;

/// <summary>
/// Represents the result of an authentication operation, including the authentication token, user information, and role
/// details.
/// </summary>
/// <remarks>This class is typically used to encapsulate the outcome of a login or authentication process. It
/// provides both success and failure results, along with relevant metadata such as the token and user
/// details.</remarks>
public class AuthResultDTO : BaseResultDTO
{
    /// <summary>
    /// Gets or sets the token used for authentication or authorization purposes.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the username associated with the user.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the role associated with the user or entity.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Creates a successful authentication result with the specified token and optional user details.
    /// </summary>
    /// <param name="token">The authentication token to include in the result. This value cannot be null or empty.</param>
    /// <param name="userName">The optional username associated with the authenticated user. Defaults to <see langword="null"/> if not
    /// provided.</param>
    /// <param name="role">The optional role of the authenticated user. Defaults to <see langword="null"/> if not provided.</param>
    /// <returns>An <see cref="AuthResultDTO"/> instance representing a successful authentication result, including the provided
    /// token and optional user details.</returns>
    public static AuthResultDTO SuccessWith(
        string token,
        string? userName = null,
        string? role = null
    ) =>
        new()
        {
            Success = true,
            Token = token,
            UserName = userName,
            Role = role,
            Message = "Login successful",
        };

    /// <summary>
    /// Creates a failed authentication result with the specified error message.
    /// </summary>
    /// <param name="message">The error message describing the reason for the failure. Cannot be null or empty.</param>
    /// <returns>An <see cref="AuthResultDTO"/> instance representing the failure.</returns>
    public static AuthResultDTO Fail(string message) => Fail<AuthResultDTO>(message);
}