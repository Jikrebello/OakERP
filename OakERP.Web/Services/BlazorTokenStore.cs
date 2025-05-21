using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OakERP.Common.Abstractions;

namespace OakERP.Web.Services;

/// <summary>
/// Provides a mechanism for storing, retrieving, and deleting authentication tokens using Blazor's <see
/// cref="ProtectedLocalStorage"/>.
/// </summary>
/// <remarks>This class is designed to securely manage authentication tokens in a Blazor application by leveraging
/// the <see cref="ProtectedLocalStorage"/> service for local storage with encryption.</remarks>
/// <param name="storage"></param>
public class BlazorTokenStore(ProtectedLocalStorage storage) : ITokenStore
{
    public async Task SaveTokenAsync(string token) => await storage.SetAsync("authToken", token);

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await storage.GetAsync<string>("authToken");
            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task DeleteTokenAsync() => await storage.DeleteAsync("authToken");
}