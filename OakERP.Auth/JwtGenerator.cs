using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace OakERP.Auth;

public class JwtGenerator(IOptions<JwtOptions> jwtOptionsAccessor) : IJwtGenerator
{
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
