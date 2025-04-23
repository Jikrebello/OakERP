using OakERP.Shared.DTOs.Auth;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

namespace OakERP.Tests.Integration.Auth;

[Collection("TransactionalDB")]
public class TransactionalAuthTests(TransactionalDbFixture fixture)
    : IClassFixture<TransactionalDbFixture>
{
    [Fact]
    public async Task RegisterAsync_Should_Create_Tenant_And_License()
    {
        // Arrange
        var authService = AuthTestFactory.Create(fixture.DbContext);
        var dto = new RegisterDTO
        {
            Email = "newuser@oak.test",
            Password = "TestPass123",
            ConfirmPassword = "TestPass123",
            TenantName = "TestTenant",
        };

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        result.Success.ShouldBeTrue();

        var tenant = fixture.DbContext.Tenants.FirstOrDefault(t => t.Name == "TestTenant");
        tenant.ShouldNotBeNull();
        tenant.License.ShouldNotBeNull();
        tenant.License.ExpiryDate.ShouldNotBeNull();
    }
}

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
}