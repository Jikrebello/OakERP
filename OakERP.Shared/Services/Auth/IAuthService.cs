using OakERP.Common.DTOs.Auth;

namespace OakERP.Shared.Services.Auth;

public interface IAuthService
{
    Task<AuthResultDTO?> LoginAsync(LoginDTO loginDTO);

    Task<AuthResultDTO?> RegisterAsync(RegisterDTO registerDTO);
}