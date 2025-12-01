using OakERP.Common.Abstractions;

namespace OakERP.Services;

/// <summary>
/// Provides a secure storage mechanism for saving, retrieving, and deleting authentication tokens.
/// </summary>
/// <remarks>This class uses platform-specific secure storage to persist tokens, ensuring they are stored
/// securely. It is designed to be used in applications that require token-based authentication.</remarks>
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