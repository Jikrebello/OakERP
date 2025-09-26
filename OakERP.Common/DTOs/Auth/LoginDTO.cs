namespace OakERP.Common.DTOs.Auth;

/// <summary>
/// Represents the data transfer object for user login credentials.
/// </summary>
/// <remarks>This class is typically used to encapsulate the email and password provided by a user during the
/// login process. It is intended to be passed to authentication services or APIs.</remarks>
public class LoginDTO
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;
}
