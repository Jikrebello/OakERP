using System.Net.Http.Headers;
using OakERP.Common.Abstractions;

namespace OakERP.Shared.Services.Api;

/// <summary>
/// A message handler that adds a Bearer token to the Authorization header of outgoing HTTP requests.
/// </summary>
/// <remarks>This handler retrieves the token from the provided <see cref="ITokenStore"/> implementation and adds
/// it to the  Authorization header of requests, except for requests targeting paths that contain "/register" or
/// "/login".</remarks>
/// <param name="tokenStore">An implementation of <see cref="ITokenStore"/> used to retrieve authentication tokens for outgoing HTTP requests.</param>
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
