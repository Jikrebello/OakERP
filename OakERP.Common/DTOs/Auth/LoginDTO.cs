namespace OakERP.Common.DTOs.Auth;

/// <summary>
/// Represents the data transfer object for user login credentials.
/// </summary>
/// <remarks>This class is typically used to encapsulate the email and password provided by a user during the
/// login process. It is intended to be passed to authentication services or APIs.</remarks>
public class LoginDTO
{
    /// <summary>
    /// Gets or sets the email address associated with the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the password associated with the user or system.
    /// </summary>
    public string Password { get; set; } = default!;
}