using System.Text;
using System.Text.Json;

namespace OakERP.Tests.Integration.TestSetup.Helpers;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value
    )
    {
        var content = new StringContent(
            JsonSerializer.Serialize(value),
            Encoding.UTF8,
            "application/json"
        );
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content };
        return await client.SendAsync(request);
    }
}
