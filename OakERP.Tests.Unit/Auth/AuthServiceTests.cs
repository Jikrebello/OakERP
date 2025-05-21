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

/// <summary>
/// Provides unit tests for the <see cref="AuthService"/> class, verifying its behavior in various authentication and
/// authorization scenarios.
/// </summary>
/// <remarks>This test class includes unit tests for the <see cref="AuthService"/> methods, such as
/// <c>RegisterAsync</c> and <c>LoginAsync</c>,  ensuring correct handling of user registration, login, and
/// tenant-related validations. Mock dependencies are used to isolate the  behavior of the <see cref="AuthService"/>
/// from external systems like the database, user manager, and tenant repository.</remarks>
public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly AuthService _authService;
    private readonly Mock<ITenantRepository> _tenantRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthServiceTests"/> class.
    /// </summary>
    /// <remarks>This constructor sets up the necessary mocks and dependencies required for testing the  <see
    /// cref="AuthService"/> class. It initializes mocked instances of <see cref="UserManager{TUser}"/>,  <see
    /// cref="SignInManager{TUser}"/>, <see cref="IJwtGenerator"/>, and <see cref="ITenantRepository"/>  to facilitate
    /// unit testing without relying on external dependencies.</remarks>
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

    /// <summary>
    /// Tests that the <see cref="IAuthService.RegisterAsync"/> method fails when the provided password and confirmation
    /// password do not match.
    /// </summary>
    /// <remarks>This test verifies that the <see cref="IAuthService.RegisterAsync"/> method returns a failure
    /// result with an appropriate error message when the <see cref="RegisterDTO.Password"/> and <see
    /// cref="RegisterDTO.ConfirmPassword"/> properties have different values.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Tests that the <c>RegisterAsync</c> method fails when attempting to register a user with an email address that
    /// already exists.
    /// </summary>
    /// <remarks>This test verifies that the <c>RegisterAsync</c> method returns a failure result with an
    /// appropriate error message when the provided email address is already associated with an existing user.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Verifies that the <c>RegisterAsync</c> method fails when user creation fails during registration.
    /// </summary>
    /// <remarks>This test ensures that the <c>RegisterAsync</c> method correctly handles a failure scenario
    /// where the user creation process returns a failed <c>IdentityResult</c>. It validates that the  method returns a
    /// failure response with the appropriate error message.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Tests that the <see cref="IAuthService.RegisterAsync"/> method succeeds when provided with valid registration
    /// data.
    /// </summary>
    /// <remarks>This test verifies that the registration process completes successfully when the input data
    /// is valid,  ensuring that a new user is created and a token is returned. It mocks dependencies such as the user
    /// manager  to simulate expected behavior.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Tests that the <see cref="IAuthService.LoginAsync"/> method fails when provided with invalid credentials.
    /// </summary>
    /// <remarks>This test verifies that the <see cref="IAuthService.LoginAsync"/> method returns a failed
    /// result  and an appropriate error message when the supplied email and password do not match a valid user
    /// account.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Verifies that the <c>LoginAsync</c> method fails when the specified user is not found.
    /// </summary>
    /// <remarks>This test ensures that the <c>LoginAsync</c> method returns a failure result with an
    /// appropriate error message when the user cannot be located by their email address.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Verifies that the <c>LoginAsync</c> method fails when the tenant associated with the user is not found.
    /// </summary>
    /// <remarks>This test ensures that the <c>LoginAsync</c> method returns a failure result with an
    /// appropriate error message when the tenant corresponding to the user's <c>TenantId</c> does not exist in the
    /// system.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Tests that the <c>LoginAsync</c> method fails when no license is found for the tenant.
    /// </summary>
    /// <remarks>This test verifies that the <c>LoginAsync</c> method returns a failure result with an
    /// appropriate error message  when the tenant associated with the user does not have a valid license. It ensures
    /// that the authentication process  enforces license validation for tenants.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Tests that the <c>LoginAsync</c> method fails when the tenant's license has expired.
    /// </summary>
    /// <remarks>This test verifies that the <c>LoginAsync</c> method returns a failure result with an
    /// appropriate  error message when the license associated with the tenant is no longer valid due to
    /// expiration.</remarks>
    /// <returns></returns>
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

    /// <summary>
    /// Tests that the <see cref="AuthService.LoginAsync"/> method succeeds and returns a valid token when provided with
    /// valid login credentials.
    /// </summary>
    /// <remarks>This test verifies the behavior of the <see cref="AuthService.LoginAsync"/> method under
    /// normal conditions, ensuring that a successful login attempt returns a non-empty token and sets the  success flag
    /// to <see langword="true"/>.</remarks>
    /// <returns></returns>
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