using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding;
using OakERP.Tests.Integration.TestSetup.Helpers;

namespace OakERP.Tests.Integration.TestSetup;

public abstract class WebApiIntegrationTestBase
{
    protected OakErpWebFactory Factory = null!;
    protected HttpClient Client = null!;

    protected readonly string TestId = Guid.NewGuid().ToString("N")[..8];

    [SetUp]
    public async Task BaseSetUp()
    {
        await TestDatabaseReset.ResetAsync();

        Factory = new OakErpWebFactory();

        using (var scope = Factory.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<SeedCoordinator>();
            await seeder.RunAsync("Testing");
        }

        Client = Factory.CreateClient();
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        Client?.Dispose();
        if (Factory is not null)
            await Factory.DisposeAsync();
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest payload)
    {
        var response = await Client.PostAsJsonAsync(route, payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiTestException(
                $"API call failed with status {response.StatusCode}: {body}",
                response.StatusCode,
                body
            );
        }
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return result!;
    }

    protected async Task<TResponse?> PostAllowingErrorAsync<TRequest, TResponse>(
        string route,
        TRequest payload
    )
    {
        var response = await Client.PostAsJsonAsync(route, payload);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    protected async Task<TResponse> GetAsync<TResponse>(string route)
    {
        var response = await Client.GetAsync(route);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiTestException(
                $"API call failed with status {response.StatusCode}: {body}",
                response.StatusCode,
                body
            );
        }
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return result!;
    }

    protected async Task DeleteAsync(string route)
    {
        var response = await Client.DeleteAsync(route);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiTestException(
                $"API DELETE failed with status {response.StatusCode}: {body}",
                response.StatusCode,
                body
            );
        }
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(string route, TRequest payload)
    {
        var response = await Client.PutAsJsonAsync(route, payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiTestException(
                $"API PUT failed with status {response.StatusCode}: {body}",
                response.StatusCode,
                body
            );
        }
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return result!;
    }

    protected async Task<TResponse> PatchAsync<TRequest, TResponse>(string route, TRequest payload)
    {
        var response = await Client.PatchAsJsonAsync(route, payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiTestException(
                $"API PATCH failed with status {response.StatusCode}: {body}",
                response.StatusCode,
                body
            );
        }
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return result!;
    }

    // -----------------------------
    // Safe DbContext helpers
    // -----------------------------
    // Use these when you need to assert directly against the DB.
    protected async Task WithDbAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(db);
    }

    protected async Task<TResult> WithDbAsync<TResult>(
        Func<ApplicationDbContext, Task<TResult>> action
    )
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(db);
    }
}
