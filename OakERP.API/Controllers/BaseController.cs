using Microsoft.AspNetCore.Mvc;
using OakERP.Common.Dtos.Base;

namespace OakERP.API.Controllers;

/// <summary>
/// Base API controller that standardizes response formatting using BaseResultDto.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns a standardized API response based on success.
    /// </summary>
    /// <typeparam name="T">Any type inheriting from BaseResultDto</typeparam>
    /// <param name="result">The result Dto from a service</param>
    /// <returns>Properly formatted IActionResult</returns>
    protected IActionResult ApiResult<T>(T result)
        where T : BaseResultDto
    {
        var statusCode = result.StatusCode.GetValueOrDefault();
        if (statusCode == 0)
        {
            statusCode = 500;
        }

        return result.Success ? Ok(result) : StatusCode(statusCode, result);
    }
}
