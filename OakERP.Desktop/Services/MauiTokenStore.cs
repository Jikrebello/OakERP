using OakERP.Common.Abstractions;

namespace OakERP.Services;

public class MauiTokenStore : ITokenStore
{
    public async Task SaveToken(string token) =>
        await SecureStorage.Default.SetAsync("authToken", token);

    public async Task<string?> GetToken() => await SecureStorage.Default.GetAsync("authToken");

    public async Task DeleteToken() => SecureStorage.Default.Remove("authToken");
}
