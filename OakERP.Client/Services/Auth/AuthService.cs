using OakERP.Client.ApiRoutes;
using OakERP.Client.Services.Api;
using OakERP.Common.DTOs.Auth;

namespace OakERP.Client.Services.Auth;

public class AuthService(IApiClient api) : IAuthService
{
    public async Task<ApiResult<AuthResultDTO>> LoginAsync(LoginDTO loginDTO)
    {
        return await api.PostAsync<LoginDTO, AuthResultDTO>(AuthRoutes.Login, loginDTO);
    }

    public async Task<ApiResult<AuthResultDTO>> RegisterAsync(RegisterDTO registerDTO)
    {
        return await api.PostAsync<RegisterDTO, AuthResultDTO>(AuthRoutes.Register, registerDTO);
    }
}
