using OakERP.Common.DTOs.Auth;
using OakERP.Shared.ApiRoutes;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.Services.Auth;

public class AuthService(IApiClient api) : IAuthService
{
    public async Task<AuthResultDTO?> LoginAsync(string email, string password)
    {
        var dto = new LoginDTO { Email = email, Password = password };
        var result = await api.PostAsync<LoginDTO, AuthResultDTO>(AuthRoutes.Login, dto);
        return result.Data;
    }
}
