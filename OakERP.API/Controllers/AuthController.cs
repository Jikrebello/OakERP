using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OakERP.API.Runtime;
using OakERP.Auth;
using OakERP.Common.Dtos.Auth;

namespace OakERP.API.Controllers;

/// <summary>
/// Provides endpoints for user authentication, including registration and login functionality.
/// </summary>
/// <remarks>This controller handles user authentication-related operations such as registering new users and
/// logging in existing users. It interacts with the authentication service to perform these operations and returns
/// appropriate HTTP responses based on the outcome.</remarks>
/// <param name="authService"></param>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : BaseApiController
{
    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <remarks>This action is accessible anonymously and does not require authentication.</remarks>
    /// <param name="Dto">The data transfer object containing the user's registration details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the registration operation.  Returns <see
    /// cref="OkObjectResult"/> if the registration is successful, or  <see cref="BadRequestObjectResult"/> if the
    /// registration fails.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitSettings.PolicyName)]
    public async Task<IActionResult> Register(RegisterDto Dto)
    {
        var result = await authService.RegisterAsync(Dto);

        return ApiResult(result);
    }

    /// <summary>
    /// Authenticates a user based on the provided login credentials.
    /// </summary>
    /// <remarks>This method allows anonymous access and is intended to be used for user
    /// authentication.</remarks>
    /// <param name="Dto">The data transfer object containing the user's login credentials.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the login operation.  Returns <see
    /// cref="OkObjectResult"/> with the result if authentication is successful;  otherwise, returns <see
    /// cref="UnauthorizedObjectResult"/> with the result.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitSettings.PolicyName)]
    public async Task<IActionResult> Login(LoginDto Dto)
    {
        var result = await authService.LoginAsync(Dto);

        return ApiResult(result);
    }
}
