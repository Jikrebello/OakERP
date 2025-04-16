using Microsoft.AspNetCore.Identity;
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

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
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
            return result.Succeeded
                ? AuthResultDTO.SuccessResult("login-success")
                : AuthResultDTO.Failed("Invalid login credentials.");
        }
    }
}