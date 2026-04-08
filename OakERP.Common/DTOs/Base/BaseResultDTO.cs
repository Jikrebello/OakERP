using System.Net;

namespace OakERP.Common.Dtos.Base;

/// <summary>
/// Represents a base class for result objects, encapsulating the success status and an optional message.
/// </summary>
/// <remarks>This class provides a common structure for result objects, including a success indicator and a
/// message for additional context. It also includes static factory methods for creating standardized success and
/// failure results.</remarks>
public abstract class BaseResultDto
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public int? StatusCode { get; set; }

    /// <summary>
    /// Creates a new instance of the specified result type with a success status and an optional message.
    /// </summary>
    /// <typeparam name="T">The type of the result, which must derive from <see cref="BaseResultDto"/> and have a parameterless constructor.</typeparam>
    /// <param name="message">An optional message to include in the result. Can be <see langword="null"/>.</param>
    /// <returns>A new instance of type <typeparamref name="T"/> with <see cref="BaseResultDto.Success"/> set to <see
    /// langword="true"/> and <see cref="BaseResultDto.Message"/> set to the specified <paramref name="message"/>.</returns>
    public static T Ok<T>(string? message = null)
        where T : BaseResultDto, new()
    {
        return new T { Success = true, Message = message };
    }

    /// <summary>
    /// Creates a new instance of the specified result type with a failure state.
    /// </summary>
    /// <typeparam name="T">The type of the result object, which must inherit from <see cref="BaseResultDto"/> and have a parameterless
    /// constructor.</typeparam>
    /// <param name="message">The error message describing the failure.</param>
    /// <param name="statusCode">The HTTP status code associated with the failure.</param>
    /// <returns>A new instance of type <typeparamref name="T"/> with <c>Success</c> set to <see langword="false"/>,
    /// <c>Message</c> set to the specified <paramref name="message"/>, and <c>StatusCode</c> set to the integer value
    /// of <paramref name="statusCode"/>.</returns>
    public static T Fail<T>(string message, HttpStatusCode statusCode)
        where T : BaseResultDto, new()
    {
        return new T
        {
            Success = false,
            Message = message,
            StatusCode = (int)statusCode,
        };
    }
}
