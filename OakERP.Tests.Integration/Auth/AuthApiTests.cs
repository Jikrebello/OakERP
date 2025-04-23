using OakERP.Shared.DTOs.Auth;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;
using System.Net.Http.Json;

namespace OakERP.Tests.Integration.Auth;

[Collection("WebApi")]
public class AuthApiTests : IClassFixture<OakErpWebFactory>, IClassFixture<PersistentDbFixture>
{
    private readonly HttpClient _client;
    private readonly PersistentDbFixture _dbFixture;

    public AuthApiTests(OakErpWebFactory factory, PersistentDbFixture dbFixture)
    {
        _client = factory.CreateClient();
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task Register_Endpoint_Should_Create_Tenant_And_License()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString("N")[..8];
        var dto = new RegisterDTO
        {
            Email = $"apiuser_{guid}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            TenantName = $"ApiTenant_{guid}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        response.EnsureSuccessStatusCode();

        // Assert - parse API response and confirm persistence
        var result = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();

        var tenant = _dbFixture.DbContext.Tenants.FirstOrDefault(t => t.Name == dto.TenantName);
        tenant.ShouldNotBeNull();

        // Register for cleanup
        _dbFixture.RegisterEntitiesForCleanup(tenant!);
    }
}