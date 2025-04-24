using System.Net.Http.Json;
using NUnit.Framework;
using OakERP.Tests.Integration.TestSetup.Fixtures;
using OakERP.Tests.Integration.TestSetup.Helpers;

namespace OakERP.Tests.Integration.TestSetup;

/// <summary>
/// Base class for all WebAPI integration tests.
/// Handles setup, teardown, HttpClient, and cleanup registration.
/// </summary>
public abstract class WebApiIntegrationTestBase
{
    protected OakErpWebFactory Factory;
    protected PersistentDbFixture DbFixture;
    protected HttpClient Client;
    private List<object> _entitiesToCleanup;

    [SetUp]
    public async Task BaseSetUp()
    {
        Factory = new OakErpWebFactory();
        DbFixture = new PersistentDbFixture();
        await DbFixture.SetUp();
        Client = Factory.CreateClient();
        _entitiesToCleanup = [];
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        // Always runs, even if test fails
        if (_entitiesToCleanup.Count != 0)
            DbFixture.RegisterEntitiesForCleanup(_entitiesToCleanup.ToArray());
        await DbFixture.TearDown();
        Client.Dispose();
        Factory.Dispose();
    }

    /// <summary>
    /// Register entities for cleanup at any point in your test.
    /// Always call IMMEDIATELY after creation/saving to ensure cleanup even on test failure.
    /// </summary>
    protected void MarkForCleanup(params object[] entities)
    {
        _entitiesToCleanup.AddRange(entities);
    }

    protected Infrastructure.Persistence.ApplicationDbContext DbContext => DbFixture.DbContext;

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
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return result;
    }

    protected async Task<TResponse> PostAndMarkAsync<TRequest, TResponse, TEntity>(
        string route,
        TRequest payload,
        Func<TRequest, TResponse, TEntity?> getEntity
    )
        where TEntity : class
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
        var entity = getEntity(payload, result!);
        if (entity != null)
            MarkForCleanup(entity);

        return result!;
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

    protected async Task<TResponse> PutAndMarkAsync<TRequest, TResponse, TEntity>(
        string route,
        TRequest payload,
        Func<TRequest, TResponse, TEntity?> getEntity
    )
        where TEntity : class
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
        var entity = getEntity(payload, result!);
        if (entity != null)
            MarkForCleanup(entity);

        return result!;
    }

    protected async Task<TResponse> PatchAndMarkAsync<TRequest, TResponse, TEntity>(
        string route,
        TRequest payload,
        Func<TRequest, TResponse, TEntity?> getEntity
    )
        where TEntity : class
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
        var entity = getEntity(payload, result!);
        if (entity != null)
            MarkForCleanup(entity);

        return result!;
    }
}