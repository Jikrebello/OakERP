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
