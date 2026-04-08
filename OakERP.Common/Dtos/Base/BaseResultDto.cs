using System.Net;

namespace OakERP.Common.Dtos.Base;

public abstract class BaseResultDto
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public int? StatusCode { get; set; }

    public static T Ok<T>(string? message = null)
        where T : BaseResultDto, new()
    {
        return new T { Success = true, Message = message };
    }

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
