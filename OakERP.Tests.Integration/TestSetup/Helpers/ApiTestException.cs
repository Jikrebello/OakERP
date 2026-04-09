using System.Net;

namespace OakERP.Tests.Integration.TestSetup.Helpers;

/// <summary>
/// Represents an exception that occurs during an API test, providing details about the HTTP status code and response
/// body.
/// </summary>
/// <remarks>This exception is typically thrown when an API test encounters an error response.  It includes the
/// HTTP status code and the response body to help diagnose the issue.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="ApiTestException"/> class with a specified error message, HTTP
/// status code, and optional response body.
/// </remarks>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="statusCode">The HTTP status code associated with the error.</param>
/// <param name="body">The optional response body returned by the API, or <see langword="null"/> if no body is available.</param>
public class ApiTestException(string message, HttpStatusCode statusCode, string? body)
    : Exception(message)
{
    /// <summary>
    /// Gets the HTTP status code returned by the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;

    /// <summary>
    /// Gets the body of the HTTP response as a string.
    /// </summary>
    public string? ResponseBody { get; } = body;
}
