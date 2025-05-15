namespace OakERP.Common.DTOs.Base;

public abstract class BaseResultDTO
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static T Fail<T>(string message)
        where T : BaseResultDTO, new()
    {
        return new T { Success = false, Message = message };
    }

    public static T Ok<T>(string? message = null)
        where T : BaseResultDTO, new()
    {
        return new T { Success = true, Message = message };
    }
}
