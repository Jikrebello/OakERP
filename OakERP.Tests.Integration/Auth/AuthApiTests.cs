using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.Common.DTOs.Auth;
using OakERP.Domain.Entities.Users;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

namespace OakERP.Tests.Integration.Auth;

/// <summary>
/// Provides integration tests for authentication-related API endpoints, including user registration and login
/// functionality.
/// </summary>
/// <remarks>This test class verifies the behavior of the authentication API endpoints, such as user registration
/// and login, under various conditions. It ensures that the endpoints function correctly and handle edge cases, such as
/// invalid input, duplicate data, and license-related constraints.  The tests use a combination of HTTP requests and
/// database assertions to validate the expected outcomes. Each test is self-contained and cleans up any created
/// resources to maintain test isolation.</remarks>
[TestFixture]
public class AuthApiTests : WebApiIntegrationTestBase
{
    /// <summary>
    /// Tests the registration endpoint to ensure that a new tenant and associated license are created successfully.
    /// </summary>
    /// <remarks>This test verifies that the registration process creates a tenant with the specified name and
    /// associates a license with the tenant. It uses a unique identifier to ensure test isolation and checks the
    /// database for the expected entities after the operation.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Register_Endpoint_Should_Create_Tenant_And_License()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = $"apiuser_{TestId}@oak.test",
            Password = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{TestId}",
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
        var tenant = await DbFixture.DbContext.Tenants.FirstOrDefaultAsync(t =>
            t.Name == dto.TenantName
        );
        tenant.ShouldNotBeNull();
        var license = await DbFixture.DbContext.Licenses.FirstOrDefaultAsync(l =>
            l.TenantId == tenant!.Id
        );
        license.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that the <c>Register</c> endpoint fails when the provided password and confirmation password do not match.
    /// </summary>
    /// <remarks>This test verifies that the registration process enforces password confirmation by ensuring
    /// that the  <c>Success</c> property of the result is <see langword="false"/> and the error message indicates the
    /// mismatch.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Register_Endpoint_Should_Fail_If_Passwords_Do_Not_Match()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = $"badpw_{TestId}@oak.test",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            Password = "GoodPass123!",
            ConfirmPassword = "MismatchPass123!",
            TenantName = $"BadPwTenant_{TestId}",
        };

        // Act
        var result = await PostAllowingErrorAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            dto
        );

        // Assert
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldContain("Passwords do not match");
    }

    /// <summary>
    /// Tests that the registration endpoint fails when attempting to register a user with an email address that already
    /// exists in the system, even if the tenant name is different.
    /// </summary>
    /// <remarks>This test verifies that the API enforces unique email addresses across tenants during user
    /// registration. It ensures that a second registration attempt with the same email but a different tenant name
    /// results in a failure response, with an appropriate error message indicating the email conflict.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Register_Endpoint_Should_Fail_If_Email_Already_Exists()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = $"duplicate_{TestId}@oak.test",
            Password = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            ConfirmPassword = "TestPass123!",
            TenantName = $"DupTenant_{TestId}",
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
            FirstName = "TestFirstname2",
            LastName = "TestLastname2",
            PhoneNumber = "987654321",
            ConfirmPassword = "TestPass123!",
            TenantName = $"DupTenant2_{TestId}",
        };

        // Act
        var result2 = await PostAllowingErrorAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            dto2
        );

        // Assert
        result2.ShouldNotBeNull();
        result2.Success.ShouldBeFalse();
        result2.Message.ShouldContain("Email already exists");

        // Cleanup second tenant if it somehow got created (defensive)
        var tenant2 = await DbContext.Tenants.FirstOrDefaultAsync(t => t.Name == dto2.TenantName);
        if (tenant2 != null)
            MarkForCleanup(tenant2);
    }

    /// <summary>
    /// Tests that the login endpoint successfully authenticates a user with valid credentials.
    /// </summary>
    /// <remarks>This test verifies that a user can log in after registering with valid credentials.  It
    /// ensures that the login endpoint returns a successful response, including a non-empty authentication
    /// token.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Login_Endpoint_Should_Succeed_With_Valid_Credentials()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = $"api_login_{TestId}@oak.test",
            Password = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{TestId}",
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

    /// <summary>
    /// Verifies that the login endpoint fails when an invalid password is provided.
    /// </summary>
    /// <remarks>This test ensures that the login process does not succeed when the provided password  does
    /// not match the registered user's password. It validates that the API returns a  failure response, ensuring proper
    /// authentication behavior.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_With_Invalid_Password()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = $"api_badpass_{TestId}@oak.test",
            Password = "TestPass123!",
            FirstName = "TestFirstName",
            LastName = "TestLastName",
            PhoneNumber = "1234567890",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{TestId}",
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

    /// <summary>
    /// Tests that the login endpoint fails when provided with a nonexistent email address.
    /// </summary>
    /// <remarks>This test verifies that the login endpoint returns a failure response when attempting to log
    /// in  with an email address that does not exist in the system. It ensures that the endpoint correctly  handles
    /// invalid credentials and does not allow unauthorized access.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_With_Nonexistent_Email()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = $"doesnotexist_{TestId}@oak.test",
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

    /// <summary>
    /// Verifies that the login endpoint fails with an appropriate error message when the tenant's license has expired.
    /// </summary>
    /// <remarks>This test ensures that a user cannot log in if the associated tenant's license is no longer
    /// valid.  It simulates the scenario by registering a user, expiring the tenant's license, and attempting to log
    /// in. The expected behavior is that the login attempt fails and returns a message indicating the license has
    /// expired.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_If_License_Expired()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = $"api_expired_{TestId}@oak.test",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{TestId}",
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
        var tenant = await DbFixture
            .DbContext.Tenants.Include(t => t.License)
            .FirstOrDefaultAsync(t => t.Name == registerDto.TenantName);

        tenant.ShouldNotBeNull();
        tenant!.License.ShouldNotBeNull();

        tenant.License.ExpiryDate = DateTime.UtcNow.AddDays(-1);
        await DbFixture.DbContext.SaveChangesAsync();

        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        // Act
        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeFalse();
        loginResult.Message.ShouldBe("License has expired.");
    }

    /// <summary>
    /// Verifies that the login endpoint fails when a tenant does not have an assigned license.
    /// </summary>
    /// <remarks>This test ensures that the login operation returns a failure response if the tenant
    /// associated  with the user attempting to log in does not have a valid license assigned. The test simulates  this
    /// scenario by registering a user, removing the license from the associated tenant, and then  attempting to log
    /// in.</remarks>
    /// <returns></returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_If_No_License_Assigned()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = $"api_nolicense_{TestId}@oak.test",
            Password = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{TestId}",
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
        var tenant = await DbFixture
            .DbContext.Tenants.Include(t => t.License)
            .FirstOrDefaultAsync(t => t.Name == registerDto.TenantName);
        tenant.ShouldNotBeNull();

        var license = tenant!.License;
        if (license != null)
        {
            DbFixture.DbContext.Licenses.Remove(license);
            await DbFixture.DbContext.SaveChangesAsync();
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
        loginResult.Message.ShouldContain("License not found for tenant.");
    }
}