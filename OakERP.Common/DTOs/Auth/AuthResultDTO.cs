using System.Net;
using OakERP.Common.Dtos.Base;

namespace OakERP.Common.Dtos.Auth;

/// <summary>
/// Represents the result of an authentication operation, including the authentication token, user information, and role
/// details.
/// </summary>
/// <remarks>This class is typically used to encapsulate the outcome of a login or authentication process. It
/// provides both success and failure results, along with relevant metadata such as the token and user
/// details.</remarks>
public class AuthResultDto : BaseResultDto
{
    public string? Token { get; set; }

    public string? UserName { get; set; }

    public string? Role { get; set; }

    public static AuthResultDto SuccessWith(
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

    public static AuthResultDto Fail(string message, HttpStatusCode statusCode) =>
        Fail<AuthResultDto>(message, statusCode);
}
