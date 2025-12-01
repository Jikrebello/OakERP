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
    /// Verifies that the register endpoint creates a new tenant and associated license when provided with valid
    /// registration data.
    /// </summary>
    /// <remarks>This test ensures that both the API response and the database state reflect successful tenant
    /// and license creation after registration. The test checks that the registration result is successful and that the
    /// tenant and license records exist in the database.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Register_Endpoint_Should_Create_Tenant_And_License()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = $"apiuser_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            TenantName = $"ApiTenant_{TestId}",
        };

        // Act
        var result = await PostAsync<RegisterDTO, AuthResultDTO>(ApiRoutes.Auth.Register, dto);

        // Assert (API)
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();

        // Assert (DB)
        await WithDbAsync(async db =>
        {
            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Name == dto.TenantName);
            tenant.ShouldNotBeNull("Tenant should be created by Register endpoint.");

            var license = await db.Licenses.FirstOrDefaultAsync(l => l.TenantId == tenant!.Id);
            license.ShouldNotBeNull("License should be created and linked to the Tenant.");
        });
    }

    /// <summary>
    /// Verifies that the registration endpoint returns a failure response when the provided password and confirmation
    /// password do not match.
    /// </summary>
    /// <remarks>This test ensures that the API enforces password confirmation validation by rejecting
    /// registration attempts with mismatched passwords. The response is expected to indicate failure and include a
    /// message specifying the mismatch.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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
    /// Verifies that the registration endpoint fails when attempting to register a user with an email address that
    /// already exists in the system.
    /// </summary>
    /// <remarks>This test ensures that duplicate email addresses are not allowed during user registration,
    /// even if the registration is attempted for a different tenant. It also verifies that no new tenant is created
    /// when registration fails due to a duplicate email.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Register_Endpoint_Should_Fail_If_Email_Already_Exists()
    {
        // Arrange (first registration)
        var dto = new RegisterDTO
        {
            Email = $"duplicate_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            TenantName = $"DupTenant_{TestId}",
        };

        // Act 1: create initial user/tenant
        var first = await PostAsync<RegisterDTO, AuthResultDTO>(ApiRoutes.Auth.Register, dto);
        first.Success.ShouldBeTrue();

        // Sanity check: tenant exists
        await WithDbAsync(async db =>
        {
            var t = await db.Tenants.FirstOrDefaultAsync(x => x.Name == dto.TenantName);
            t.ShouldNotBeNull();
        });

        // Arrange (second registration with SAME email, different tenant)
        var dto2 = new RegisterDTO
        {
            Email = dto.Email, // duplicate email
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "TestFirstname2",
            LastName = "TestLastname2",
            PhoneNumber = "987654321",
            TenantName = $"DupTenant2_{TestId}",
        };

        // Act 2: attempt duplicate email
        var result2 = await PostAllowingErrorAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            dto2
        );

        // Assert
        result2.ShouldNotBeNull();
        result2!.Success.ShouldBeFalse();
        result2.Message.ShouldContain("Email already exists");

        // Optional: verify that the second tenant did NOT get created
        await WithDbAsync(async db =>
        {
            var t2 = await db.Tenants.FirstOrDefaultAsync(x => x.Name == dto2.TenantName);
            t2.ShouldBeNull();
        });
    }

    /// <summary>
    /// Verifies that the login endpoint successfully authenticates a user when provided with valid credentials.
    /// </summary>
    /// <remarks>This test first registers a new user and then attempts to log in with the same credentials.
    /// The test asserts that the login operation succeeds and returns a non-empty authentication token.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Login_Endpoint_Should_Succeed_With_Valid_Credentials()
    {
        // Arrange: first register a user
        var registerDto = new RegisterDTO
        {
            Email = $"api_login_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            TenantName = $"ApiTenant_{TestId}",
        };

        var registerResult = await PostAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            registerDto
        );

        registerResult.ShouldNotBeNull();
        registerResult.Success.ShouldBeTrue();

        // Act: attempt login with the same credentials
        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        var loginResult = await PostAsync<LoginDTO, AuthResultDTO>(ApiRoutes.Auth.Login, loginDto);

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeTrue();
        loginResult.Token.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that the login endpoint returns a failure response when an invalid password is provided for an existing
    /// user.
    /// </summary>
    /// <remarks>This test first registers a new user with valid credentials, then attempts to log in using
    /// the correct email but an incorrect password. The test asserts that the login attempt fails and that the response
    /// message indicates invalid credentials.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_With_Invalid_Password()
    {
        // Arrange: create a valid user first
        var registerDto = new RegisterDTO
        {
            Email = $"api_badpass_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "TestFirstName",
            LastName = "TestLastName",
            PhoneNumber = "1234567890",
            TenantName = $"ApiTenant_{TestId}",
        };

        var registerResult = await PostAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            registerDto
        );

        registerResult.ShouldNotBeNull();
        registerResult.Success.ShouldBeTrue();

        // Attempt login with wrong password
        var loginDto = new LoginDTO { Email = registerDto.Email, Password = "WrongPassword!" };

        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert: should fail
        loginResult.ShouldNotBeNull();
        loginResult!.Success.ShouldBeFalse();
        loginResult.Message.ShouldContain("Invalid login credentials");
    }

    /// <summary>
    /// Verifies that the login endpoint returns a failed result when attempting to log in with an email address that
    /// does not exist.
    /// </summary>
    /// <remarks>This test ensures that the authentication API does not succeed when provided with credentials
    /// for a nonexistent user. It is intended to validate correct error handling and response structure for invalid
    /// login attempts.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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
    /// Verifies that the login endpoint returns a failure response when a tenant's license has expired.
    /// </summary>
    /// <remarks>This test registers a new tenant, manually expires its license, and then attempts to log in.
    /// The test asserts that the login attempt fails and the response message indicates the license has
    /// expired.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_If_License_Expired()
    {
        // Arrange: register a user/tenant
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

        var registerResult = await PostAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            registerDto
        );

        registerResult.ShouldNotBeNull();
        registerResult.Success.ShouldBeTrue();

        // Expire the license directly in DB
        await WithDbAsync(async db =>
        {
            var tenant = await db
                .Tenants.Include(t => t.License)
                .FirstOrDefaultAsync(t => t.Name == registerDto.TenantName);

            tenant.ShouldNotBeNull();
            tenant!.License.ShouldNotBeNull();

            tenant.License!.ExpiryDate = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        });

        // Act: try to log in
        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult!.Success.ShouldBeFalse();
        loginResult.Message.ShouldBe("License has expired.");
    }

    /// <summary>
    /// Verifies that the login endpoint returns a failure response when a user attempts to log in without an assigned
    /// license for their tenant.
    /// </summary>
    /// <remarks>This test ensures that the system enforces license requirements by preventing login for
    /// tenants without a valid license. It first registers a user and tenant, removes the tenant's license, and then
    /// asserts that login fails with an appropriate error message.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Login_Endpoint_Should_Fail_If_No_License_Assigned()
    {
        // Arrange: register a user/tenant
        var registerDto = new RegisterDTO
        {
            Email = $"api_nolicense_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "TestFirstname",
            LastName = "TestLastname",
            PhoneNumber = "123456789",
            TenantName = $"ApiTenant_{TestId}",
        };

        var registerResult = await PostAsync<RegisterDTO, AuthResultDTO>(
            ApiRoutes.Auth.Register,
            registerDto
        );

        registerResult.ShouldNotBeNull();
        registerResult.Success.ShouldBeTrue();

        // Remove the tenant's license
        await WithDbAsync(async db =>
        {
            var tenant = await db
                .Tenants.Include(t => t.License)
                .FirstOrDefaultAsync(t => t.Name == registerDto.TenantName);

            tenant.ShouldNotBeNull();

            if (tenant!.License is not null)
            {
                db.Licenses.Remove(tenant.License);
                await db.SaveChangesAsync();
            }
        });

        // Act: try to log in
        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        var loginResult = await PostAllowingErrorAsync<LoginDTO, AuthResultDTO>(
            ApiRoutes.Auth.Login,
            loginDto
        );

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult!.Success.ShouldBeFalse();
        loginResult.Message.ShouldContain("License not found for tenant.");
    }
}
