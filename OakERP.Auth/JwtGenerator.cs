using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OakERP.Domain.Entities;

namespace OakERP.Auth;

/// <summary>
/// Provides functionality for generating JSON Web Tokens (JWTs) for authenticated users.
/// </summary>
/// <remarks>This class generates JWTs using the HMAC-SHA256 algorithm and retrieves configuration settings such
/// as the signing key, issuer, audience, and expiration time from the application's configuration. The generated tokens
/// include claims for the user's ID, email, and tenant ID.</remarks>
public class JwtGenerator : IJwtGenerator
{
    private readonly IConfiguration _config;

    public JwtGenerator(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified user.
    /// </summary>
    /// <remarks>The method retrieves JWT configuration settings, including the signing key, issuer, audience,
    /// and expiration time, from the application's configuration. The signing key must be at least  32 characters long
    /// to ensure compatibility with HMAC-SHA256.</remarks>
    /// <param name="user">The user for whom the JWT is being generated. Must not be null.</param>
    /// <returns>A string representation of the generated JWT, which includes claims such as the user's ID, email,  and tenant
    /// ID, and is signed using HMAC-SHA256.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the signing key specified in the configuration is less than 32 characters long.</exception>
    public string Generate(ApplicationUser user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        var keyString = jwtSettings["Key"]!;
        if (keyString.Length < 32)
            throw new InvalidOperationException(
                "JWT Key must be at least 32 characters for HMAC-SHA256."
            );

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("tenantId", user.TenantId.ToString()),
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"]));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}