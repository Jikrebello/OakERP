using OakERP.Common.Abstractions;

namespace OakERP.Shared.Hosts.Maui;

public sealed class MauiTokenStore : ITokenStore
{
    private const string TokenKey = "authToken";

    public async Task SaveTokenAsync(string token) =>
        await SecureStorage.Default.SetAsync(TokenKey, token);

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync(TokenKey);

    public Task DeleteTokenAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }
}
