using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OakERP.API.Runtime;
using OakERP.Common.Dtos.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitSettings.PolicyName)]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Register a new tenant admin user.",
        Description = "Creates the tenant, initial license, and first admin account for a new OakERP tenant."
    )]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);

        return ApiResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitSettings.PolicyName)]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Authenticate an existing user.",
        Description = "Validates the supplied credentials and returns a JWT plus current user details."
    )]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);

        return ApiResult(result);
    }
}
