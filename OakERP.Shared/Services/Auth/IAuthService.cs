using OakERP.Common.DTOs.Auth;

namespace OakERP.Shared.Services.Auth;

public interface IAuthService
{
    Task<AuthResultDTO?> LoginAsync(string email, string password);
}
