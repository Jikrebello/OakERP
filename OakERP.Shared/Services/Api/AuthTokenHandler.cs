using System.Net.Http.Headers;
using OakERP.Common.Abstractions;

namespace OakERP.Shared.Services.Api;

public class AuthTokenHandler(ITokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var path = request.RequestUri?.AbsolutePath;

        if (path != null && (path.Contains("/register") || path.Contains("/login")))
            return await base.SendAsync(request, cancellationToken);

        var token = await tokenStore.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
