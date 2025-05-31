using Microsoft.AspNetCore.Mvc;
using OakERP.Common.DTOs.Base;

namespace OakERP.API.Controllers;

/// <summary>
/// Base API controller that standardizes response formatting using BaseResultDTO.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns a standardized API response based on success.
    /// </summary>
    /// <typeparam name="T">Any type inheriting from BaseResultDTO</typeparam>
    /// <param name="result">The result DTO from a service</param>
    /// <returns>Properly formatted IActionResult</returns>
    protected IActionResult ApiResult<T>(T result)
        where T : BaseResultDTO
    {
        var statusCode = result.StatusCode == 0 ? 500 : result.StatusCode;

        return result.Success ? Ok(result) : StatusCode((int)statusCode, result);
    }
}
