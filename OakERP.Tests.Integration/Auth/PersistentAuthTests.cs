using OakERP.Shared.DTOs.Auth;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

namespace OakERP.Tests.Integration.Auth;

[Collection("PersistentDB")]
public class PersistentAuthTests(PersistentDbFixture fixture) : IClassFixture<PersistentDbFixture>
{
    [Fact]
    public async Task RegisterAsync_Should_Create_Tenant_And_License()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var email = $"testuser_{guid}@oak.test";
        var tenantName = $"TestTenant_{guid}";

        var authService = AuthTestFactory.Create(fixture.DbContext);

        var dto = new RegisterDTO
        {
            Email = email,
            Password = "TestPass123",
            ConfirmPassword = "TestPass123",
            TenantName = tenantName,
        };

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeTrue();

        var tenant = fixture.DbContext.Tenants.FirstOrDefault(t => t.Name == tenantName);
        tenant.ShouldNotBeNull();
        tenant.License.ShouldNotBeNull();
        tenant.License.ExpiryDate.ShouldNotBeNull();

        // Clean
        fixture.RegisterEntitiesForCleanup(tenant);
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_If_Email_Already_Exists()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var email = $"dupeuser_{guid}@oak.test";
        var tenantName1 = $"TenantA_{guid}";
        var tenantName2 = $"TenantB_{guid}";

        var authService = AuthTestFactory.Create(fixture.DbContext);

        // Register the first user (success)
        var registerDto1 = new RegisterDTO
        {
            Email = email,
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            TenantName = tenantName1,
        };
        var regResult1 = await authService.RegisterAsync(registerDto1);
        regResult1.Success.ShouldBeTrue();

        // Attempt to register another user with the same email (should fail)
        var registerDto2 = new RegisterDTO
        {
            Email = email,
            Password = "AnotherPass123",
            ConfirmPassword = "AnotherPass123",
            TenantName = tenantName2,
        };
        var regResult2 = await authService.RegisterAsync(registerDto2);

        // Assert
        regResult2.Success.ShouldBeFalse();
        regResult2.Error.ShouldBe("Email already exists.");

        // Clean up
        var tenant = fixture.DbContext.Tenants.FirstOrDefault(t => t.Name == tenantName1);
        fixture.RegisterEntitiesForCleanup(tenant!);
    }

    [Fact]
    public async Task RegisterAsync_Should_Fail_When_Passwords_Do_Not_Match()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var email = $"nomatch_{guid}@oak.test";
        var tenantName = $"TenantNoMatch_{guid}";

        var authService = AuthTestFactory.Create(fixture.DbContext);

        var dto = new RegisterDTO
        {
            Email = email,
            Password = "Pass123!",
            ConfirmPassword = "DifferentPass456!",
            TenantName = tenantName,
        };

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeFalse();
        result.Error.ShouldBe("Passwords do not match.");

        // Optional Clean up (shouldn't be needed as nothing is created, but for paranoia):
        var tenant = fixture.DbContext.Tenants.FirstOrDefault(t => t.Name == tenantName);
        if (tenant != null)
            fixture.RegisterEntitiesForCleanup(tenant);
    }

    //[Fact]
    //public async Task LoginAsync_Should_Succeed_With_Valid_Credentials()
    //{
    //    // Arrange
    //    var guid = Guid.NewGuid().ToString("N")[..8];
    //    var email = $"loginuser_{guid}@oak.test";
    //    var tenantName = $"LoginTenant_{guid}";

    //    var authService = AuthTestFactory.Create(fixture.DbContext);

    //    var registerDto = new RegisterDTO
    //    {
    //        Email = email,
    //        Password = "SuperSecret123",
    //        ConfirmPassword = "SuperSecret123",
    //        TenantName = tenantName,
    //    };
    //    var registerResult = await authService.RegisterAsync(registerDto);
    //    registerResult.Success.ShouldBeTrue();

    //    // Act
    //    var loginDto = new LoginDTO { Email = email, Password = "SuperSecret123" };
    //    var loginResult = await authService.LoginAsync(loginDto);

    //    // Assert
    //    loginResult.Success.ShouldBeTrue();
    //    loginResult.Token.ShouldNotBeNullOrWhiteSpace();

    //    // Clean up
    //    var tenant = fixture.DbContext.Tenants.FirstOrDefault(t => t.Name == tenantName);
    //    fixture.RegisterEntitiesForCleanup(tenant!);
    //}
}