using System.Net.Http.Json;
using OakERP.Shared.DTOs.Auth;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

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
            TenantName = $"ApiTenant_{guid}",
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        response.EnsureSuccessStatusCode();

        // Assert
        var result = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        result.ShouldNotBeNull();
        result.Success.ShouldBeTrue();
        var tenant = _dbFixture.DbContext.Tenants.FirstOrDefault(t => t.Name == dto.TenantName);
        tenant.ShouldNotBeNull();
        var license = _dbFixture.DbContext.Licenses.FirstOrDefault(l => l.TenantId == tenant!.Id);
        license.ShouldNotBeNull();

        // Register for cleanup
        _dbFixture.RegisterEntitiesForCleanup(tenant!);
    }

    [Fact]
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

        // Register the user first
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        regResponse.EnsureSuccessStatusCode();

        var tenant = _dbFixture.DbContext.Tenants.FirstOrDefault(t =>
            t.Name == registerDto.TenantName
        );
        tenant.ShouldNotBeNull();

        var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDTO>();

        // Assert
        loginResult.ShouldNotBeNull();
        loginResult.Success.ShouldBeTrue();
        loginResult.Token.ShouldNotBeNullOrEmpty();

        // Cleanup
        _dbFixture.RegisterEntitiesForCleanup(tenant!);
    }
}