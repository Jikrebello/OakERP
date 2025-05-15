using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.Auth;
using OakERP.Shared.DTOs.Auth;

namespace OakERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        var result = await authService.RegisterAsync(dto);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var result = await authService.LoginAsync(dto);

        return result.Success ? Ok(result) : Unauthorized(result);
    }
}