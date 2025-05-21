using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OakERP.Common.Abstractions;

namespace OakERP.Web.Services;

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