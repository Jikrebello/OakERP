using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("whoami")]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Get the current authenticated user.",
        Description = "Returns the authenticated user id, email address, and tenant identifier from the current claims principal."
    )]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult WhoAmI()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
        var email = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var tenantId = User.FindFirstValue("tenantId") ?? "Unknown";

        return Ok(new CurrentUserResponse(userId, email, tenantId));
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Access the admin-only probe endpoint.",
        Description = "Returns a simple confirmation message when the current user is authenticated in the Admin role."
    )]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminOnly()
    {
        return Ok("Welcome, Admin 🎩.");
    }

    [HttpGet("user-only")]
    [Authorize(Roles = "User")]
    [SwaggerOperation(
        Summary = "Access the user-only probe endpoint.",
        Description = "Returns a simple confirmation message when the current user is authenticated in the User role."
    )]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult UserOnly()
    {
        return Ok("Welcome, user 🧰.");
    }

    public sealed record CurrentUserResponse(string UserId, string Email, string TenantId);
}
