using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OakERP.Domain.Entities;
using OakERP.Infrastructure.Persistence;
using OakERP.Shared.DTOs.Auth;

namespace OakERP.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly IJwtGenerator _jwtGenerator;

    private readonly IConfiguration _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext db,
        IConfiguration config,
        IJwtGenerator jwtGenerator
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _config = config;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return AuthResultDTO.Failed("Passwords do not match.");

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return AuthResultDTO.Failed("Email already exists.");

        var tenant = new Tenant
        {
            Name = dto.TenantName,
            License = new License
            {
                Key = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
            },
        };

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

        var tenant = await _db
            .Tenants.Include(t => t.License)
            .FirstOrDefaultAsync(t => t.Id == user.TenantId);

        if (tenant == null)
        {
            return AuthResultDTO.Failed("Tenant not found.");
        }

        if (tenant.License == null)
        {
            return AuthResultDTO.Failed("License not found for tenant.");
        }

        if (tenant.License.ExpiryDate is not null && tenant.License.ExpiryDate < DateTime.UtcNow)
        {
            return AuthResultDTO.Failed("License has expired.");
        }

        var token = _jwtGenerator.Generate(user);

        return AuthResultDTO.SuccessResult(token);
    }
}