using System.Net.Http.Json;
using NUnit.Framework;
using OakERP.Tests.Integration.TestSetup.Fixtures;
using OakERP.Tests.Integration.TestSetup.Helpers;

namespace OakERP.Tests.Integration.TestSetup;

/// <summary>
/// Provides a base class for integration tests targeting Web API endpoints.
/// </summary>
/// <remarks>This class simplifies the setup and teardown of integration tests by managing the test server,
/// database context, and HTTP client. It also provides utility methods for common API operations such as sending HTTP
/// requests and handling entity cleanup to ensure proper test isolation.</remarks>
public abstract class WebApiIntegrationTestBase
{
    protected OakErpWebFactory Factory;
    protected PersistentDbFixture DbFixture;
    protected HttpClient Client;
    private List<object> _entitiesToCleanup;
    protected readonly string TestId = Guid.NewGuid().ToString("N")[..8];

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
        await Factory.DisposeAsync();
    }

    /// <summary>
    /// Marks the specified entities for cleanup by adding them to an internal tracking list.
    /// </summary>
    /// <remarks>This method is typically used to track entities that require cleanup at a later stage.  The
    /// caller is responsible for ensuring that the provided entities are valid and not null.</remarks>
    /// <param name="entities">An array of objects to be marked for cleanup. Each object represents an entity that will be processed during
    /// cleanup.</param>
    protected void MarkForCleanup(params object[] entities)
    {
        _entitiesToCleanup.AddRange(entities);
    }

    protected Infrastructure.Persistence.ApplicationDbContext DbContext => DbFixture.DbContext;

    /// <summary>
    /// Sends a POST request to the specified route with the provided payload and deserializes the response.
    /// </summary>
    /// <remarks>This method uses JSON serialization for the request payload and response content. Ensure that
    /// the types <typeparamref name="TRequest"/> and <typeparamref name="TResponse"/> are compatible with JSON
    /// serialization.</remarks>
    /// <typeparam name="TRequest">The type of the payload to be sent in the request body.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the API.</typeparam>
    /// <param name="route">The relative route of the API endpoint to which the request is sent.</param>
    /// <param name="payload">The payload to include in the request body. Cannot be null.</param>
    /// <returns>The deserialized response of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ApiTestException">Thrown if the API call fails with a non-success status code. The exception contains the status code and response
    /// body.</exception>
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

    /// <summary>
    /// Sends a POST request with the specified payload to the given route and deserializes the response to the
    /// specified type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the payload to be sent in the POST request.</typeparam>
    /// <typeparam name="TResponse">The type to which the response content will be deserialized.</typeparam>
    /// <param name="route">The relative route to which the POST request is sent. Cannot be null or empty.</param>
    /// <param name="payload">The payload to include in the POST request body. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized response of type
    /// <typeparamref name="TResponse"/> if the response content is successfully deserialized; otherwise, <see
    /// langword="null"/>.</returns>
    protected async Task<TResponse?> PostAllowingErrorAsync<TRequest, TResponse>(
        string route,
        TRequest payload
    )
    {
        var response = await Client.PostAsJsonAsync(route, payload);
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return result;
    }

    /// <summary>
    /// Sends a POST request to the specified route with the provided payload, processes the response,  and optionally
    /// marks an entity for cleanup based on the provided mapping function.
    /// </summary>
    /// <remarks>This method is designed to facilitate API testing by handling common tasks such as sending a
    /// POST request,  deserializing the response, and managing test cleanup for entities created during the
    /// test.</remarks>
    /// <typeparam name="TRequest">The type of the request payload sent to the API.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the API.</typeparam>
    /// <typeparam name="TEntity">The type of the entity to be marked for cleanup, if applicable.</typeparam>
    /// <param name="route">The API route to which the POST request is sent.</param>
    /// <param name="payload">The request payload to be serialized and sent in the POST request.</param>
    /// <param name="getEntity">A function that maps the request payload and response to an entity of type <typeparamref name="TEntity"/>.  If
    /// the function returns a non-null entity, it will be marked for cleanup.</param>
    /// <returns>The deserialized response of type <typeparamref name="TResponse"/> from the API.</returns>
    /// <exception cref="ApiTestException">Thrown if the API call fails with a non-success status code. The exception contains the status code  and the
    /// response body for debugging purposes.</exception>
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

    /// <summary>
    /// Sends an asynchronous GET request to the specified route and deserializes the response content into the
    /// specified type.
    /// </summary>
    /// <remarks>The method expects the response content to be in JSON format and deserializable into the
    /// specified type <typeparamref name="TResponse"/>.</remarks>
    /// <typeparam name="TResponse">The type to which the response content will be deserialized.</typeparam>
    /// <param name="route">The relative URI of the API endpoint to send the GET request to.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized response content of
    /// type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ApiTestException">Thrown if the API call fails with a non-success HTTP status code. The exception contains the status code and
    /// response body.</exception>
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

    /// <summary>
    /// Sends an asynchronous DELETE request to the specified route and throws an exception if the response indicates
    /// failure.
    /// </summary>
    /// <param name="route">The relative URI of the resource to delete. This cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ApiTestException">Thrown if the DELETE request fails with a non-success HTTP status code. The exception contains the status code
    /// and response body.</exception>
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

    /// <summary>
    /// Sends a PUT request with the specified payload, processes the response, and optionally marks an entity for
    /// cleanup.
    /// </summary>
    /// <remarks>If the <paramref name="getEntity"/> function returns a non-<see langword="null"/> entity, the
    /// entity will be marked for cleanup using the <c>MarkForCleanup</c> method.</remarks>
    /// <typeparam name="TRequest">The type of the request payload sent in the PUT request.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the PUT request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity to be marked for cleanup, if applicable.</typeparam>
    /// <param name="route">The API route to which the PUT request is sent.</param>
    /// <param name="payload">The payload to include in the PUT request.</param>
    /// <param name="getEntity">A function that takes the request payload and the response, and returns an entity to be marked for cleanup. If
    /// the function returns <see langword="null"/>, no entity will be marked.</param>
    /// <returns>The deserialized response of type <typeparamref name="TResponse"/> from the PUT request.</returns>
    /// <exception cref="ApiTestException">Thrown if the PUT request fails with a non-success HTTP status code. The exception includes the status code and
    /// the response body for debugging purposes.</exception>
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

    /// <summary>
    /// Sends a PATCH request to the specified route with the provided payload, processes the response,  and optionally
    /// marks an entity for cleanup based on the provided mapping function.
    /// </summary>
    /// <remarks>This method is designed to facilitate API testing by sending a PATCH request, processing the
    /// response,  and optionally marking entities for cleanup to ensure proper resource management during
    /// tests.</remarks>
    /// <typeparam name="TRequest">The type of the request payload sent in the PATCH request.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the PATCH request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity to be marked for cleanup, if applicable.</typeparam>
    /// <param name="route">The API route to which the PATCH request is sent.</param>
    /// <param name="payload">The payload to be sent in the PATCH request.</param>
    /// <param name="getEntity">A function that maps the request payload and response to an entity of type <typeparamref name="TEntity"/>.  If
    /// the function returns a non-null entity, it will be marked for cleanup.</param>
    /// <returns>The deserialized response of type <typeparamref name="TResponse"/> from the PATCH request.</returns>
    /// <exception cref="ApiTestException">Thrown if the PATCH request fails, including the HTTP status code and response body in the exception.</exception>
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