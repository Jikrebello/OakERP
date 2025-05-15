using Microsoft.AspNetCore.Identity;
using OakERP.Common.DTOs.Auth;
using OakERP.Domain.Entities;
using OakERP.Domain.Repositories;

namespace OakERP.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly ITenantRepository _tenantRepository;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtGenerator jwtGenerator,
        ITenantRepository tenantRepository
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGenerator = jwtGenerator;
        _tenantRepository = tenantRepository;
    }

    public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return AuthResultDTO.Fail("Passwords do not match.");

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return AuthResultDTO.Fail("Email already exists.");

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

        await _tenantRepository.CreateAsync(tenant);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            TenantId = tenant.Id,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return AuthResultDTO.Fail(result.Errors.First().Description);

        var token = _jwtGenerator.Generate(user);
        return AuthResultDTO.SuccessWith(token, userName: user.UserName);
    }

    public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            dto.Email,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: false
        );

        if (!result.Succeeded)
            return AuthResultDTO.Fail("Invalid login credentials.");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return AuthResultDTO.Fail("User not found.");

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
        if (tenant is null)
            return AuthResultDTO.Fail("Tenant not found.");

        if (tenant.License is null)
            return AuthResultDTO.Fail("License not found for tenant.");

        if (tenant.License.ExpiryDate is not null && tenant.License.ExpiryDate < DateTime.UtcNow)
            return AuthResultDTO.Fail("License has expired.");

        var token = _jwtGenerator.Generate(user);

        var primaryRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

        return AuthResultDTO.SuccessWith(token, userName: user.UserName, role: primaryRole);
    }
}
