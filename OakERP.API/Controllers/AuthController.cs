using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OakERP.API.Runtime;
using OakERP.Common.Dtos.Auth;

namespace OakERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitSettings.PolicyName)]
    public async Task<IActionResult> Register(RegisterDto Dto)
    {
        var result = await authService.RegisterAsync(Dto);

        return ApiResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitSettings.PolicyName)]
    public async Task<IActionResult> Login(LoginDto Dto)
    {
        var result = await authService.LoginAsync(Dto);

        return ApiResult(result);
    }
}
