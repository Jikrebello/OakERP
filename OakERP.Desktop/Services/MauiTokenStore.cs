using OakERP.Common.Abstractions;

namespace OakERP.Services;

public class MauiTokenStore : ITokenStore
{
    public async Task SaveTokenAsync(string token) =>
        await SecureStorage.Default.SetAsync("authToken", token);

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync("authToken");

    public async Task DeleteTokenAsync() => SecureStorage.Default.Remove("authToken");
}
