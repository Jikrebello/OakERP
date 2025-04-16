using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OakERP.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
        var email = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var tenantId = User.FindFirstValue("tenantId") ?? "Unknown";

        return Ok(
            new
            {
                userId,
                email,
                tenantId,
            }
        );
    }
}