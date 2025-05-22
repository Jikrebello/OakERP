using OakERP.Common.DTOs.Auth;
using OakERP.Shared.ApiRoutes;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.Services.Auth;

public class AuthService(IApiClient api) : IAuthService
{
    public async Task<AuthResultDTO?> LoginAsync(LoginDTO loginDTO)
    {
        var result = await api.PostAsync<LoginDTO, AuthResultDTO>(AuthRoutes.Login, loginDTO);
        return result.Data;
    }

    public async Task<AuthResultDTO?> RegisterAsync(RegisterDTO registerDTO)
    {
        var result = await api.PostAsync<RegisterDTO, AuthResultDTO>(
            AuthRoutes.Register,
            registerDTO
        );
        return result.Data;
    }
}