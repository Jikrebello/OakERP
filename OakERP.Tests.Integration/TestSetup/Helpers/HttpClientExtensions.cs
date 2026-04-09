using System.Text;
using System.Text.Json;

namespace OakERP.Tests.Integration.TestSetup.Helpers;

public static class HttpClientExtensions
{
    /// <summary>
    /// Sends a PATCH request with a JSON-encoded body to the specified URI.
    /// </summary>
    /// <remarks>The request body is serialized to JSON using <see cref="JsonSerializer"/>
    /// and sent with a content type of "application/json". Ensure the server supports PATCH requests  with JSON
    /// payloads.</remarks>
    /// <typeparam name="T">The type of the object to serialize into the JSON request body.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> instance used to send the request.</param>
    /// <param name="requestUri">The URI to which the PATCH request is sent.</param>
    /// <param name="value">The object to serialize into the JSON request body.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the  <see
    /// cref="HttpResponseMessage"/> returned by the server.</returns>
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
