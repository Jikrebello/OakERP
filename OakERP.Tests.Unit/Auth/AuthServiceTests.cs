using Microsoft.AspNetCore.Identity;
using Moq;
using OakERP.Auth;
using OakERP.Common.DTOs.Auth;
using OakERP.Common.Persistence;
using OakERP.Domain.Entities.Users;
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
    private readonly AuthServiceTestFactory _factory;

    public AuthServiceTests()
    {
        _factory = new AuthServiceTestFactory();
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
        var service = _factory.CreateService();

        // Act
        var result = await service.RegisterAsync(dto);

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

        _factory
            .UserManager.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new ApplicationUser());
        var service = _factory.CreateService();

        // Act
        var result = await service.RegisterAsync(dto);

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

        _factory
            .UserManager.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _factory
            .UserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Invalid password." })
            );

        _factory
            .TenantRepository.Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .Returns(Task.CompletedTask);

        // Mock SaveChangesAsync to simulate successful transaction commit
        _factory
            .UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid password.");
    }

    /// <summary>
    /// Tests that the <c>RegisterAsync</c> method successfully registers a new user when valid data is provided.
    /// </summary>
    /// <remarks>This test verifies that the <c>RegisterAsync</c> method performs the following actions: <list
    /// type="bullet"> <item>Ensures the user does not already exist by checking their email.</item> <item>Creates a new
    /// user with the provided password.</item> <item>Assigns the user to the "Admin" role.</item> <item>Creates a new
    /// tenant associated with the user.</item> </list> The test asserts that the operation succeeds and a valid token
    /// is returned.</remarks>
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

        _factory
            .UserManager.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _factory
            .UserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _factory
            .UserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRoles.Admin))
            .ReturnsAsync(IdentityResult.Success);

        _factory
            .TenantRepository.Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .Returns(Task.CompletedTask);

        _factory
            .UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        // Act
        var result = await service.RegisterAsync(dto);

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

        _factory
            .SignInManager.Setup(s => s.PasswordSignInAsync(dto.Email, dto.Password, false, false))
            .ReturnsAsync(SignInResult.Failed);
        var service = _factory.CreateService();

        // Act
        var result = await service.LoginAsync(dto);

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

        _factory
            .SignInManager.Setup(s =>
                s.CheckPasswordSignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                )
            )
            .ReturnsAsync(SignInResult.Success);

        _factory
            .UserManager.Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);
        var service = _factory.CreateService();

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
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
            UserName = dto.Email,
        };

        _factory
            .UserManager.Setup(u => u.FindByEmailAsync(dto.Email.Trim()))
            .ReturnsAsync(fakeUser);

        _factory
            .SignInManager.Setup(s =>
                s.CheckPasswordSignInAsync(fakeUser, dto.Password, It.IsAny<bool>())
            )
            .ReturnsAsync(SignInResult.Success);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(fakeUser.TenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Tenant)null!);

        var service = _factory.CreateService();

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Tenant not found.");

        // Optional: ensure we never issued a token
        _factory.JwtGenerator.Verify(j => j.Generate(It.IsAny<ApplicationUser>()), Times.Never);

        _factory.SignInManager.Verify(
            s => s.CheckPasswordSignInAsync(fakeUser, dto.Password, It.IsAny<bool>()),
            Times.Once
        );
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

        _factory
            .SignInManager.Setup(s =>
                s.CheckPasswordSignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                )
            )
            .ReturnsAsync(SignInResult.Success);

        _factory.UserManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(tenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tenant);

        var service = _factory.CreateService();

        // Act
        var result = await service.LoginAsync(dto);

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

        _factory
            .SignInManager.Setup(s =>
                s.CheckPasswordSignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                )
            )
            .ReturnsAsync(SignInResult.Success);

        _factory.UserManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(tenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tenant);

        var service = _factory.CreateService();

        // Act
        var result = await service.LoginAsync(dto);

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

        _factory
            .SignInManager.Setup(s =>
                s.CheckPasswordSignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                )
            )
            .ReturnsAsync(SignInResult.Success);

        _factory.UserManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);

        _factory.UserManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(["User"]);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(tenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tenant);

        var service = _factory.CreateService();

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.Success.ShouldBeTrue();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }
}