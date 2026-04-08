using OakERP.Client.Services.Api;
using OakERP.Common.Dtos.Auth;

namespace OakERP.Client.Services.Auth;

public interface IAuthService
{
    Task<ApiResult<AuthResultDto>> LoginAsync(LoginDto loginDto);

    Task<ApiResult<AuthResultDto>> RegisterAsync(RegisterDto registerDto);
}
