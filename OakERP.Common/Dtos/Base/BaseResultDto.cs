using System.Text.Json.Serialization;
using OakERP.Common.Errors;

namespace OakERP.Common.Dtos.Base;

public abstract class BaseResultDto
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public int? StatusCode { get; set; }

    [JsonIgnore]
    public string? ErrorCode { get; set; }

    [JsonIgnore]
    public FailureKind? FailureKind { get; set; }

    public static T Ok<T>(string? message = null)
        where T : BaseResultDto, new()
    {
        return new T { Success = true, Message = message };
    }

    public static T Fail<T>(string code, string message, FailureKind kind)
        where T : BaseResultDto, new()
    {
        return new T
        {
            Success = false,
            Message = message,
            ErrorCode = code,
            FailureKind = kind,
        };
    }

    public static T Fail<T>(ResultError error)
        where T : BaseResultDto, new() => Fail<T>(error.Code, error.Message, error.Kind);
}
