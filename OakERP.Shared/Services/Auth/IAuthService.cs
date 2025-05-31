using OakERP.Common.DTOs.Auth;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.Services.Auth;

public interface IAuthService
{
    Task<ApiResult<AuthResultDTO>> LoginAsync(LoginDTO loginDTO);

    Task<ApiResult<AuthResultDTO>> RegisterAsync(RegisterDTO registerDTO);
}
