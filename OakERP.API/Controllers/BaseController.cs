using Microsoft.AspNetCore.Mvc;
using OakERP.Common.Dtos.Base;

namespace OakERP.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
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
