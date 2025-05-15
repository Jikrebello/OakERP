using OakERP.Common.DTOs.Base;

namespace OakERP.Common.DTOs.Auth;

public class AuthResultDTO : BaseResultDTO
{
    public string? Token { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }

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

    public static AuthResultDTO Fail(string message) => Fail<AuthResultDTO>(message);
}
