using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OakERP.Common.Abstractions;

namespace OakERP.Web.Services;

public class BlazorTokenStore(ProtectedLocalStorage storage) : ITokenStore
{
    public async Task SaveToken(string token) => await storage.SetAsync("authToken", token);

    public async Task<string?> GetToken()
    {
        var result = await storage.GetAsync<string>("authToken");
        return result.Success ? result.Value : null;
    }

    public async Task DeleteToken() => await storage.DeleteAsync("authToken");
}
