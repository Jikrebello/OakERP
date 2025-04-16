using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OakERP.Domain.Entities;
using OakERP.Infrastructure.Persistence;
using OakERP.Shared.DTOs.Auth;

namespace OakERP.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            IConfiguration config
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _config = config;
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new("tenantId", user.TenantId.ToString()),
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(jwtSettings["ExpireMinutes"])
            );

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return AuthResultDTO.Failed("Passwords do not match.");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return AuthResultDTO.Failed("Email already exists.");

            var tenant = new Tenant { Name = dto.TenantName };

            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                TenantId = tenant.Id,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return AuthResultDTO.Failed(result.Errors.First().Description);

            return AuthResultDTO.SuccessResult("registered");
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
            var result = await _signInManager.PasswordSignInAsync(
                dto.Email,
                dto.Password,
                false,
                false
            );
            if (!result.Succeeded)
            {
                return AuthResultDTO.Failed("Invalid login credentials");
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
            {
                return AuthResultDTO.Failed("User not found.");
            }

            var token = GenerateJwtToken(user);

            return AuthResultDTO.SuccessResult(token);
        }
    }
}