using OakERP.Client.ApiRoutes;
using OakERP.Client.Services.Api;
using OakERP.Common.Dtos.Auth;

namespace OakERP.Client.Services.Auth;

public class AuthService(IApiClient api) : IAuthService
{
    public async Task<ApiResult<AuthResultDto>> LoginAsync(LoginDto loginDto)
    {
        return await api.PostAsync<LoginDto, AuthResultDto>(AuthRoutes.Login, loginDto);
    }

    public async Task<ApiResult<AuthResultDto>> RegisterAsync(RegisterDto registerDto)
    {
        return await api.PostAsync<RegisterDto, AuthResultDto>(AuthRoutes.Register, registerDto);
    }
}
