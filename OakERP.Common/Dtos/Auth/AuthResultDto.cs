using System.Net;
using OakERP.Common.Dtos.Base;
using OakERP.Common.Errors;

namespace OakERP.Common.Dtos.Auth;

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

    public static AuthResultDto Fail(ResultError error) => Fail<AuthResultDto>(error);
}
