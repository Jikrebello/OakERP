using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using OakERP.Auth;
using OakERP.Domain.Entities;
using OakERP.Infrastructure.Persistence;
using OakERP.Shared.DTOs.Auth;
using Shouldly;

namespace OakERP.Tests.Unit.Auth;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

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

        _authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            _dbContext,
            configMock.Object,
            _jwtGenerator
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
        result.Error.ShouldBe("Passwords do not match.");
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

        _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(new ApplicationUser()); // Pretend user exists

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("Email already exists.");
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

        _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null!); // No existing user

        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Invalid password." })
            );

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("Invalid password.");
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
        result.Token.ShouldBe("registered");
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
        result.Error.ShouldBe("Invalid login credentials");
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
        result.Error.ShouldBe("User not found.");
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

        _dbContext.Tenants.RemoveRange(_dbContext.Tenants); // no tenant added

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("Tenant not found.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_License_Not_Found_For_Tenant()
    {
        // Arrange
        var dto = new LoginDTO { Email = "user@example.com", Password = "correctpass" };

        var tenant = new Tenant { Name = "NoLicenseTenant" };
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        // Make sure no license exists for this tenant (should be none, but let's ensure)
        var licenses = _dbContext.Licenses.Where(l => l.TenantId == tenant.Id).ToList();
        if (licenses.Count != 0)
        {
            _dbContext.Licenses.RemoveRange(licenses);
            await _dbContext.SaveChangesAsync();
        }

        var fakeUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = tenant.Id,
        };

        _signInManager
            .Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Success);

        _userManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("License not found for tenant.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_License_Expired()
    {
        // Arrange
        var dto = new LoginDTO { Email = "expired@example.com", Password = "correctpass" };

        var tenantId = Guid.NewGuid();

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "TenantZ",
            License = new License { Key = "abc123", ExpiryDate = DateTime.UtcNow.AddDays(-1) },
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.SaveChanges();

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

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("License has expired.");
    }

    [Fact]
    public async Task LoginAsync_Should_Succeed_And_Return_Token()
    {
        // Arrange
        var dto = new LoginDTO { Email = "valid@example.com", Password = "validpass" };

        var tenantId = Guid.NewGuid();

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "TenantA",
            License = new License { Key = "abc123", ExpiryDate = DateTime.UtcNow.AddDays(10) },
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.SaveChanges();

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

        var jwtMock = new Mock<IJwtGenerator>();
        jwtMock.Setup(j => j.Generate(It.IsAny<ApplicationUser>())).Returns("mocked-jwt-token");

        var authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            _dbContext,
            new ConfigurationBuilder().Build(),
            jwtMock.Object
        );

        // Act
        var result = await authService.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeTrue();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }
}