using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
/// <c>RegisterAsync</c> and <c>LoginAsync</c>, ensuring correct handling of user registration, login, and
/// tenant-related validations. Mock dependencies are used to isolate the behavior of the <see cref="AuthService"/>
/// from external systems like the database and identity framework.</remarks>
public class AuthServiceTests
{
    private readonly AuthServiceTestFactory _factory;

    public AuthServiceTests()
    {
        _factory = new AuthServiceTestFactory();
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_Passwords_Do_Not_Match()
    {
        var dto = new RegisterDTO
        {
            Email = "test@example.com",
            Password = "123456",
            ConfirmPassword = "different",
            TenantName = "TenantA",
        };
        var service = _factory.CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Passwords do not match.");
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_Email_Already_Exists()
    {
        var dto = new RegisterDTO
        {
            Email = "existing@example.com",
            Password = "123456",
            ConfirmPassword = "123456",
            TenantName = "TenantB",
        };

        _factory
            .IdentityGateway.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new ApplicationUser());

        var service = _factory.CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Email already exists.");
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_User_Creation_Fails()
    {
        var dto = new RegisterDTO
        {
            Email = "failuser@example.com",
            Password = "bad",
            ConfirmPassword = "bad",
            TenantName = "TenantC",
        };

        _factory
            .IdentityGateway.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _factory
            .IdentityGateway.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "Invalid password." })
            );

        _factory
            .TenantRepository.Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .Returns(Task.CompletedTask);

        _factory
            .UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid password.");
    }

    [Fact]
    public async Task RegisterAsync_Should_Succeed_When_Data_Is_Valid()
    {
        var dto = new RegisterDTO
        {
            Email = "newuser@example.com",
            Password = "goodpassword",
            ConfirmPassword = "goodpassword",
            TenantName = "TenantD",
        };

        _factory
            .IdentityGateway.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _factory
            .IdentityGateway.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _factory
            .IdentityGateway.Setup(m =>
                m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRoles.Admin)
            )
            .ReturnsAsync(IdentityResult.Success);

        _factory
            .TenantRepository.Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .Returns(Task.CompletedTask);

        _factory
            .UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.ShouldBeTrue();
        result.Token.ShouldBe("mock-token");
    }

    [Fact]
    public async Task RegisterAsync_Should_Write_Audit_Log_On_Success()
    {
        var dto = new RegisterDTO
        {
            Email = "audit_register@example.com",
            Password = "goodpassword",
            ConfirmPassword = "goodpassword",
            TenantName = "AuditTenant",
        };

        _factory
            .IdentityGateway.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _factory
            .IdentityGateway.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _factory
            .IdentityGateway.Setup(m =>
                m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRoles.Admin)
            )
            .ReturnsAsync(IdentityResult.Success);

        _factory
            .TenantRepository.Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .Returns(Task.CompletedTask);

        _factory
            .UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        await service.RegisterAsync(dto);

        _factory.Logger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    HasStructuredProperty(state, "AuditAction", "UserRegistration")
                    && HasStructuredProperty(state, "AuditOutcome", "Success")
                    && HasStructuredProperty(state, "Email", dto.Email)
                    && HasStructuredProperty(state, "TenantName", dto.TenantName)
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_Credentials_Are_Invalid()
    {
        var dto = new LoginDTO { Email = "test@example.com", Password = "wrongpass" };

        var user = new ApplicationUser { Email = dto.Email, UserName = dto.Email };

        _factory.IdentityGateway.Setup(s => s.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _factory
            .IdentityGateway.Setup(s => s.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        var service = _factory.CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
    }

    [Fact]
    public async Task LoginAsync_Should_Write_Audit_Log_When_Credentials_Are_Invalid()
    {
        var dto = new LoginDTO { Email = "audit_login@example.com", Password = "wrongpass" };

        var user = new ApplicationUser { Email = dto.Email, UserName = dto.Email };

        _factory.IdentityGateway.Setup(s => s.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _factory
            .IdentityGateway.Setup(s => s.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        var service = _factory.CreateService();

        await service.LoginAsync(dto);

        _factory.Logger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    HasStructuredProperty(state, "AuditAction", "UserLogin")
                    && HasStructuredProperty(state, "AuditOutcome", "Failure")
                    && HasStructuredProperty(state, "AuditReason", "InvalidCredentials")
                    && HasStructuredProperty(state, "Email", dto.Email)
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_User_Not_Found()
    {
        var dto = new LoginDTO { Email = "missing@example.com", Password = "correctpass" };

        _factory
            .IdentityGateway.Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser)null!);

        var service = _factory.CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_Tenant_Not_Found()
    {
        var dto = new LoginDTO { Email = "user@example.com", Password = "correctpass" };
        var fakeUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = Guid.NewGuid(),
            UserName = dto.Email,
        };

        _factory
            .IdentityGateway.Setup(u => u.FindByEmailAsync(dto.Email.Trim()))
            .ReturnsAsync(fakeUser);

        _factory
            .IdentityGateway.Setup(s => s.CheckPasswordSignInAsync(fakeUser, dto.Password, false))
            .ReturnsAsync(SignInResult.Success);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(fakeUser.TenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Tenant)null!);

        var service = _factory.CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Tenant not found.");
        _factory.JwtGenerator.Verify(j => j.Generate(It.IsAny<JwtTokenInput>()), Times.Never);
        _factory.IdentityGateway.Verify(
            s => s.CheckPasswordSignInAsync(fakeUser, dto.Password, false),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_License_Not_Found_For_Tenant()
    {
        var dto = new LoginDTO { Email = "user@example.com", Password = "correctpass" };
        var tenantId = Guid.NewGuid();
        var fakeUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            TenantId = tenantId,
        };
        var tenant = new Tenant { Id = tenantId, Name = "NoLicenseTenant" };

        _factory.IdentityGateway.Setup(s => s.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _factory
            .IdentityGateway.Setup(s => s.CheckPasswordSignInAsync(fakeUser, dto.Password, false))
            .ReturnsAsync(SignInResult.Success);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(tenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tenant);

        var service = _factory.CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("License not found for tenant.");
    }

    [Fact]
    public async Task LoginAsync_Should_Fail_When_License_Expired()
    {
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

        _factory.IdentityGateway.Setup(s => s.FindByEmailAsync(dto.Email)).ReturnsAsync(fakeUser);

        _factory
            .IdentityGateway.Setup(s => s.CheckPasswordSignInAsync(fakeUser, dto.Password, false))
            .ReturnsAsync(SignInResult.Success);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(tenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tenant);

        var service = _factory.CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("License has expired.");
    }

    [Fact]
    public async Task LoginAsync_Should_Succeed_And_Return_Token()
    {
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
            UserName = dto.Email,
        };

        _factory.IdentityGateway.Setup(s => s.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _factory
            .IdentityGateway.Setup(s => s.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(SignInResult.Success);
        _factory.IdentityGateway.Setup(s => s.GetRolesAsync(user)).ReturnsAsync(["User"]);

        _factory
            .TenantRepository.Setup(r =>
                r.FindWithLicenseAsync(tenantId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(tenant);

        var service = _factory.CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.ShouldBeTrue();
        result.Token.ShouldNotBeNullOrWhiteSpace();
        _factory.JwtGenerator.Verify(
            j =>
                j.Generate(
                    It.Is<JwtTokenInput>(input =>
                        input.UserId == user.Id
                        && input.Email == user.Email
                        && input.TenantId == user.TenantId
                    )
                ),
            Times.Once
        );
    }

    private static bool HasStructuredProperty(object state, string propertyName, object? expectedValue)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> properties)
        {
            return false;
        }

        var matchedProperty = properties.FirstOrDefault(property => property.Key == propertyName);
        return Equals(matchedProperty.Value, expectedValue);
    }
}
