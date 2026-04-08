using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OakERP.API.Errors;
using OakERP.Common.Dtos.Base;

namespace OakERP.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ApiResult<T>(T result)
        where T : BaseResultDto
    {
        if (result.Success)
        {
            return Ok(result);
        }

        var statusCode = ResultStatusCodeResolver.Resolve(result);
        result.StatusCode = statusCode;
        return StatusCode(statusCode, result);
    }

    protected string ResolvePerformedBy() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? "api-user";
}
