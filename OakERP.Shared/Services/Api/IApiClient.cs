namespace OakERP.Shared.Services.Api;

/// <summary>
/// Defines methods for making HTTP requests to an API, including support for sending data via POST and retrieving data
/// via GET.
/// </summary>
/// <remarks>This interface provides generic methods for interacting with APIs, allowing callers to specify
/// request and response types. The methods return an <see cref="ApiResult{T}"/> that encapsulates the response data and
/// any associated metadata or errors.</remarks>
public interface IApiClient
{
    /// <summary>
    /// Sends an HTTP POST request to the specified URL with the provided payload and deserializes the response.
    /// </summary>
    /// <remarks>The method serializes the <paramref name="payload"/> to JSON and sends it as the request
    /// body. The server's response is expected to be in JSON format and is deserialized into an object of type
    /// <typeparamref name="TResponse"/>.</remarks>
    /// <typeparam name="TRequest">The type of the payload to be sent in the request body.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the server.</typeparam>
    /// <param name="url">The URL to which the POST request is sent. Must be a valid, non-null URI.</param>
    /// <param name="payload">The payload to include in the request body. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see
    /// cref="ApiResult{TResponse}"/> object that encapsulates the server's response, including the deserialized
    /// response data of type <typeparamref name="TResponse"/>.</returns>
    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest payload);

    /// <summary>
    /// Sends an asynchronous GET request to the specified URL and returns the response deserialized into the specified
    /// type.
    /// </summary>
    /// <typeparam name="TResponse">The type to which the response will be deserialized.</typeparam>
    /// <param name="url">The URL to send the GET request to. Must be a valid, non-null, and non-empty URI.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see
    /// cref="ApiResult{TResponse}"/> object that includes the deserialized response data and additional metadata about
    /// the request.</returns>
    Task<ApiResult<TResponse>> GetAsync<TResponse>(string url);
}
