using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OakERP.Common.Abstractions;

namespace OakERP.Web.Services;

public class BlazorTokenStore(
    ProtectedLocalStorage localStorage,
    ProtectedSessionStorage sessionStorage,
    IWebHostEnvironment env
) : ITokenStore
{
    private readonly bool _isDev = env.IsDevelopment();

    public async Task SaveTokenAsync(string token)
    {
        if (_isDev)
            await localStorage.SetAsync("authToken", token);
        else
            await sessionStorage.SetAsync("authToken", token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = _isDev
                ? await localStorage.GetAsync<string>("authToken")
                : await sessionStorage.GetAsync<string>("authToken");

            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task DeleteTokenAsync()
    {
        if (_isDev)
            await localStorage.DeleteAsync("authToken");
        else
            await sessionStorage.DeleteAsync("authToken");
    }
}