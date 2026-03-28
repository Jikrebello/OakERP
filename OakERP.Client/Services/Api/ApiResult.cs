namespace OakERP.Client.Services.Api;

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
