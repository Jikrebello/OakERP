using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using OakERP.Auth;
using OakERP.Common.DTOs.Auth;
using OakERP.Domain.Entities;
using OakERP.Domain.Repositories;
using Shouldly;

namespace OakERP.Tests.Unit.Auth;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly AuthService _authService;
    private readonly Mock<ITenantRepository> _tenantRepository;

    public AuthServiceTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManager = new Mock<SignInManager<ApplicationUser>>(
            _userManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null,
            null,
            null,
            null
        );

        var configMock = new Mock<IConfiguration>();

        var jwtMock = new Mock<IJwtGenerator>();
        jwtMock.Setup(j => j.Generate(It.IsAny<ApplicationUser>())).Returns("mock-token");
        _jwtGenerator = jwtMock.Object;

        _tenantRepository = new Mock<ITenantRepository>();

        _authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            _jwtGenerator,
            _tenantRepository.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_Passwords_Do_Not_Match()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = "test@example.com",
            Password = "123456",
            ConfirmPassword = "different",
            TenantName = "TenantA",
        };

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Passwords do not match.");
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_Email_Already_Exists()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = "existing@example.com",
            Password = "123456",
            ConfirmPassword = "123456",
            TenantName = "TenantB",
        };

        _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(new ApplicationUser());

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Email already exists.");
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_User_Creation_Fails()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = "failuser@example.com",
            Password = "bad",
            ConfirmPassword = "bad",
            TenantName = "TenantC",
        };

        _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null!);

        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Invalid password." })
            );

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid password.");
    }

    [Fact]
    public async Task RegisterAsync_Should_Succeed_When_Data_Is_Valid()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = "newuser@example.com",
            Password = "goodpassword",
            ConfirmPassword = "goodpassword",
            TenantName = "TenantD",
        };

        _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null!);

        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeTrue();
        result.Token.ShouldBe("mock-token");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_Credentials_Are_Invalid()
    {
        // Arrange
        var dto = new LoginDTO { Email = "test@example.com", Password = "wrongpass" };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_User_Not_Found()
    {
        // Arrange
        var dto = new LoginDTO { Email = "missing@example.com", Password = "correctpass" };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("User not found.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_Tenant_Not_Found()
    {
        // Arrange
        var dto = new LoginDTO { Email = "user@example.com", Password = "correctpass" };
        var fakeUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = Guid.NewGuid(),
        };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _tenantRepository.Setup(r => r.GetByIdAsync(fakeUser.TenantId)).ReturnsAsync((Tenant)null!); // Not found

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Tenant not found.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_License_Not_Found_For_Tenant()
    {
        // Arrange
        var dto = new LoginDTO { Email = "user@example.com", Password = "correctpass" };
        var tenantId = Guid.NewGuid();
        var fakeUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = tenantId,
        };
        var tenant = new Tenant { Id = tenantId, Name = "NoLicenseTenant" };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _tenantRepository.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("License not found for tenant.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_License_Expired()
    {
        // Arrange
        var dto = new LoginDTO { Email = "expired@example.com", Password = "correctpass" };
        var tenantId = Guid.NewGuid();
        var expiredLicense = new License
        {
            Key = "abc123",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
        };
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "TenantZ",
            License = expiredLicense,
        };

        var fakeUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = tenantId,
        };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _tenantRepository.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("License has expired.");
    }

    [Fact]
    public async Task LoginAsync_Should_Succeed_And_Return_Token()
    {
        // Arrange
        var dto = new LoginDTO { Email = "valid@example.com", Password = "validpass" };
        var tenantId = Guid.NewGuid();
        var validLicense = new License { Key = "abc123", ExpiryDate = DateTime.UtcNow.AddDays(10) };
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "TenantA",
            License = validLicense,
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = tenantId,
        };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);

        _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(["User"]);

        _tenantRepository.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeTrue();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }
}
