using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.Domain.Entities;
using OakERP.Shared.DTOs.Auth;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

namespace OakERP.Tests.Integration.Auth;

[TestFixture]
public class AuthApiTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Register_Endpoint_Should_Create_Tenant_And_License()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var dto = new RegisterDTO
        {
            Email = $"apiuser_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{guid}",
        };

        // Act
        var result = await PostAndMarkAsync<RegisterDTO, AuthResultDTO, Tenant>(
            ApiRoutes.Auth.Register,
            dto,
            (request, response) =>
                DbFixture.DbContext.Tenants.FirstOrDefault(t => t.Name == request.TenantName)
        );

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        var tenant = DbFixture.DbContext.Tenants.FirstOrDefault(t => t.Name == dto.TenantName);
        tenant.ShouldNotBeNull();
        var license = DbFixture.DbContext.Licenses.FirstOrDefault(l => l.TenantId == tenant!.Id);
        license.ShouldNotBeNull();
    }

    [Test]
    public async Task Register_Endpoint_Should_Fail_If_Passwords_Do_Not_Match()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = $"badpw_{Guid.NewGuid():N}@oak.test",
            Password = "GoodPass123!",
            ConfirmPassword = "MismatchPass123!",
            TenantName = $"BadPwTenant_{Guid.NewGuid():N}",
        };

        // Act
        var result = await PostAllowingErrorAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            dto
        );

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("Passwords do not match");
    }

    [Test]
    public async Task Register_Endpoint_Should_Fail_If_Email_Already_Exists()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var dto = new RegisterDTO
        {
            Email = $"duplicate_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"DupTenant_{guid}",
        };

        await PostAndMarkAsync<RegisterDTO, AuthResultDTO, Tenant>(
            ApiRoutes.Auth.Register,
            dto,
            (req, resp) => DbContext.Tenants.FirstOrDefault(t => t.Name == req.TenantName)
        );

        // Prepare second registration DTO (same email, different tenant)
        var dto2 = new RegisterDTO
        {
            Email = dto.Email,
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"DupTenant2_{guid}",
        };

        // Act
        var result2 = await PostAllowingErrorAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            dto2
        );

        // Assert
        result2.ShouldNotBeNull();
        result2.Success.ShouldBeFalse();
        result2.Error.ShouldContain("Email already exists");

        // Cleanup second tenant if it somehow got created (defensive)
        var tenant2 = DbContext.Tenants.FirstOrDefault(t => t.Name == dto2.TenantName);
        if (tenant2 != null)
            MarkForCleanup(tenant2);
    }

    [Test]
    public async Task Login_Endpoint_Should_Succeed_With_Valid_Credentials()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var registerDto = new RegisterDTO
        {
            Email = $"api_login_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{guid}",
        };

        // Register the user and mark the tenant for cleanup automatically
        await PostAndMarkAsync<RegisterDTO, AuthResultDTO, Tenant>(
            ApiRoutes.Auth.Register,
            registerDto,
            (req, resp) => DbContext.Tenants.FirstOrDefault(t => t.Name == req.TenantName)
        );

        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        // Act
        var loginResult = await PostAsync<LoginDTO, AuthResultDTO>(ApiRoutes.Auth.Login, loginDto);

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeTrue();
        loginResult.Token.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task Login_Endpoint_Should_Fail_With_Invalid_Password()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var registerDto = new RegisterDTO
        {
            Email = $"api_badpass_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{guid}",
        };

        // Register the user and mark the tenant for cleanup
        await PostAndMarkAsync<RegisterDTO, AuthResultDTO, Tenant>(
            ApiRoutes.Auth.Register,
            registerDto,
            (req, resp) => DbContext.Tenants.FirstOrDefault(t => t.Name == req.TenantName)
        );

        var loginDto = new LoginDTO { Email = registerDto.Email, Password = "WrongPassword!" };

        // Act
        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeFalse();
    }

    [Test]
    public async Task Login_Endpoint_Should_Fail_With_Nonexistent_Email()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = $"doesnotexist_{Guid.NewGuid():N}@oak.test",
            Password = "AnyPassword123!",
        };

        // Act
        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeFalse();
    }

    [Test]
    public async Task Login_Endpoint_Should_Fail_If_License_Expired()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var registerDto = new RegisterDTO
        {
            Email = $"api_expired_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{guid}",
        };

        // Register the user (and mark tenant for cleanup)
        await PostAndMarkAsync<RegisterDTO, AuthResultDTO, Tenant>(
            ApiRoutes.Auth.Register,
            registerDto,
            (req, res) =>
                DbFixture
                    .DbContext.Tenants.Include(t => t.License)
                    .FirstOrDefault(t => t.Name == req.TenantName)
        );

        // Find the tenant (with license) and expire the license
        var tenant = DbFixture
            .DbContext.Tenants.Include(t => t.License)
            .FirstOrDefault(t => t.Name == registerDto.TenantName);

        tenant.ShouldNotBeNull();
        tenant!.License.ShouldNotBeNull();

        tenant.License.ExpiryDate = DateTime.UtcNow.AddDays(-1);
        DbFixture.DbContext.SaveChanges();

        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        // Act
        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeFalse();
        loginResult.Error.ShouldBe("License has expired.");
    }

    [Test]
    public async Task Login_Endpoint_Should_Fail_If_No_License_Assigned()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var registerDto = new RegisterDTO
        {
            Email = $"api_nolicense_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{guid}",
        };

        // Register the user (and mark tenant for cleanup)
        await PostAndMarkAsync<RegisterDTO, AuthResultDTO, Tenant>(
            ApiRoutes.Auth.Register,
            registerDto,
            (req, res) =>
                DbFixture
                    .DbContext.Tenants.Include(t => t.License)
                    .FirstOrDefault(t => t.Name == req.TenantName)
        );

        // Find the tenant and remove its license
        var tenant = DbFixture
            .DbContext.Tenants.Include(t => t.License)
            .FirstOrDefault(t => t.Name == registerDto.TenantName);
        tenant.ShouldNotBeNull();

        var license = tenant!.License;
        if (license != null)
        {
            DbFixture.DbContext.Licenses.Remove(license);
            DbFixture.DbContext.SaveChanges();
        }

        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        // Act
        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeFalse();
        loginResult.Error.ShouldContain("License not found for tenant.");
    }
}