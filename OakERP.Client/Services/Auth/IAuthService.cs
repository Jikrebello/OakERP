using OakERP.Client.Services.Api;
using OakERP.Common.DTOs.Auth;

namespace OakERP.Client.Services.Auth;

public interface IAuthService
{
    Task<ApiResult<AuthResultDTO>> LoginAsync(LoginDTO loginDTO);

    Task<ApiResult<AuthResultDTO>> RegisterAsync(RegisterDTO registerDTO);
}
