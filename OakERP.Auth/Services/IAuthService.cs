using OakERP.Common.Dtos.Auth;

namespace OakERP.Auth.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto Dto);

    Task<AuthResultDto> LoginAsync(LoginDto Dto);
}
