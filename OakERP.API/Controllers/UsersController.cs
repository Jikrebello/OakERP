using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OakERP.API.Controllers;

/// <summary>
/// Provides API endpoints for managing and retrieving user-related information.
/// </summary>
/// <remarks>This controller includes endpoints for retrieving the current user's identity, as well as
/// role-specific endpoints for users with "Admin" or "User" roles. All endpoints require authentication.</remarks>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Retrieves information about the currently authenticated user.
    /// </summary>
    /// <remarks>This method returns the user's unique identifier, email address, and tenant ID based on the
    /// claims present in the current authentication context. If any of these claims are missing, their values will
    /// default to "Unknown".</remarks>
    /// <returns>An <see cref="IActionResult"/> containing a JSON object with the following properties: <list type="bullet">
    /// <item> <description><c>userId</c>: The unique identifier of the user, or "Unknown" if not
    /// available.</description> </item> <item> <description><c>email</c>: The email address of the user, or "Unknown"
    /// if not available.</description> </item> <item> <description><c>tenantId</c>: The tenant ID associated with the
    /// user, or "Unknown" if not available.</description> </item> </list></returns>
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

    /// <summary>
    /// Handles requests to the "admin-only" endpoint, accessible only to users with the "Admin" role.
    /// </summary>
    /// <remarks>This action is restricted to users with the "Admin" role, as enforced by the <see
    /// cref="AuthorizeAttribute"/>. Attempting to access this endpoint without the required role will result in an HTTP
    /// 403 Forbidden response.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing a success response with a welcome message for administrators.</returns>
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok("Welcome, Admin 🎩.");
    }

    /// <summary>
    /// Handles a request to the "user-only" endpoint, accessible only to users with the "User" role.
    /// </summary>
    /// <remarks>This action is restricted to users who are authenticated and have the "User" role.  Ensure
    /// the caller has the appropriate role assigned to access this endpoint.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing a success response with a welcome message for authorized users.</returns>
    [HttpGet("user-only")]
    [Authorize(Roles = "User")]
    public IActionResult UserOnly()
    {
        return Ok("Welcome, user 🧰.");
    }
}
