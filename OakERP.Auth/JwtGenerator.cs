using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OakERP.Domain.Entities;

namespace OakERP.Auth;

public class JwtGenerator : IJwtGenerator
{
    private readonly IConfiguration _config;

    public JwtGenerator(IConfiguration config)
    {
        _config = config;
    }

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