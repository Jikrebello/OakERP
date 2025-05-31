namespace OakERP.Shared.Services.Api;

/// <summary>
/// Represents the result of an API operation, encapsulating success status, a message,  an HTTP status code, and
/// optional data of a specified type.
/// </summary>
/// <remarks>This class provides a standardized way to represent the outcome of an API operation,  including both
/// successful and failed results. Use the static factory methods  <see cref="Ok(T, int)"/> and <see cref="Fail(string,
/// int)"/> to create instances  of this class.</remarks>
/// <typeparam name="T">The type of the data associated with the API result. This can be any type, including  <see langword="null"/> if no
/// data is returned.</typeparam>
public class ApiResult<T>
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public int StatusCode { get; init; }

    public T? Data { get; init; }

    public static ApiResult<T> Ok(T data, int statusCode = 200) =>
        new()
        {
            Success = true,
            Data = data,
            StatusCode = statusCode,
        };

    public static ApiResult<T> Fail(string message, int statusCode = 400) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
        };

    public static ApiResult<T> Fail(T? fallbackData, string message, int statusCode = 400) =>
        new()
        {
            Success = false,
            Message = message,
            Data = fallbackData,
            StatusCode = statusCode,
        };
}
