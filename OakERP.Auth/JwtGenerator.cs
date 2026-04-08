using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace OakERP.Auth;

/// <summary>
/// Provides functionality for generating JSON Web Tokens (JWTs) for authenticated users.
/// </summary>
/// <remarks>This class generates JWTs using the HMAC-SHA256 algorithm and retrieves validated settings from
/// strongly typed options. The generated tokens include claims for the user's ID, email, and tenant ID.</remarks>
public class JwtGenerator(IOptions<JwtOptions> jwtOptionsAccessor) : IJwtGenerator
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified token input.
    /// </summary>
    /// <remarks>The method uses validated JWT options for the signing key, issuer, audience, and expiration time.</remarks>
    /// <param name="input">The minimal auth-local token input required to generate the JWT.</param>
    /// <returns>A string representation of the generated JWT, which includes claims such as the user's ID, email,  and tenant
    /// ID, and is signed using HMAC-SHA256.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the signing key specified in the configuration is less than 32 characters long.</exception>
    public string Generate(JwtTokenInput input)
    {
        JwtOptions jwtOptions = jwtOptionsAccessor.Value;
        jwtOptions.Validate();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, input.UserId),
            new(JwtRegisteredClaimNames.Email, input.Email),
            new("tenantId", input.TenantId.ToString()),
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(jwtOptions.ExpireMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
