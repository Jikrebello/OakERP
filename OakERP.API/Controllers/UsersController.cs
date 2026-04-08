using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("whoami")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    public IActionResult WhoAmI()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
        var email = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var tenantId = User.FindFirstValue("tenantId") ?? "Unknown";

        return Ok(new CurrentUserResponse(userId, email, tenantId));
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult AdminOnly()
    {
        return Ok("Welcome, Admin 🎩.");
    }

    [HttpGet("user-only")]
    [Authorize(Roles = "User")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult UserOnly()
    {
        return Ok("Welcome, user 🧰.");
    }

    public sealed record CurrentUserResponse(string UserId, string Email, string TenantId);
}
