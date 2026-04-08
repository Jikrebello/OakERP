using OakERP.Common.Abstractions;

namespace OakERP.Mobile.Services;

public class MauiTokenStore : ITokenStore
{
    public async Task SaveTokenAsync(string token) =>
        await SecureStorage.Default.SetAsync("authToken", token);

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync("authToken");

    public Task DeleteTokenAsync()
    {
        SecureStorage.Default.Remove("authToken");
        return Task.CompletedTask;
    }
}
