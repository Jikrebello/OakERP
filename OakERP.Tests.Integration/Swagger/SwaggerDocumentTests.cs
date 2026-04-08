using System.Text.Json;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OakERP.Tests.Integration.TestSetup;

namespace OakERP.Tests.Integration.Swagger;

[TestFixture]
public sealed class SwaggerDocumentTests
{
    private OakErpWebFactory factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void SetUp()
    {
        factory = new OakErpWebFactory();
        client = factory.CreateClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        client.Dispose();
        await factory.DisposeAsync();
    }

    [Test]
    public async Task SwaggerJson_IsAvailable_InTestingEnvironment()
    {
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(
            response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo("application/json")
        );
    }

    [Test]
    public async Task SwaggerJson_Describes_Current_Controller_Routes_And_Security()
    {
        using var document = await GetSwaggerDocumentAsync();

        AssertRoute(document, "/api/auth/register", "post", requiresBearer: false);
        AssertRoute(document, "/api/auth/login", "post", requiresBearer: false);
        AssertRoute(document, "/api/users/whoami", "get", requiresBearer: true);
        AssertRoute(document, "/api/users/admin-only", "get", requiresBearer: true);
        AssertRoute(document, "/api/users/user-only", "get", requiresBearer: true);
        AssertRoute(document, "/api/ap-invoices", "post", requiresBearer: true);
        AssertRoute(document, "/api/ar-invoices", "post", requiresBearer: true);
        AssertRoute(document, "/api/ap-payments", "post", requiresBearer: true);
        AssertRoute(
            document,
            "/api/ap-payments/{paymentId}/allocations",
            "post",
            requiresBearer: true
        );
        AssertRoute(document, "/api/ar-receipts", "post", requiresBearer: true);
        AssertRoute(
            document,
            "/api/ar-receipts/{receiptId}/allocations",
            "post",
            requiresBearer: true
        );

        var bearerScheme = document
            .RootElement.GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");

        Assert.That(bearerScheme.GetProperty("type").GetString(), Is.EqualTo("http"));
        Assert.That(bearerScheme.GetProperty("scheme").GetString(), Is.EqualTo("bearer"));
    }

    [Test]
    public async Task SwaggerJson_Documents_Request_Bodies_And_Core_Response_Metadata()
    {
        using var document = await GetSwaggerDocumentAsync();

        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/auth/register",
            "post",
            "RegisterDto",
            "AuthResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/auth/login",
            "post",
            "LoginDto",
            "AuthResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/ap-invoices",
            "post",
            "CreateApInvoiceCommand",
            "ApInvoiceCommandResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/ar-invoices",
            "post",
            "CreateArInvoiceCommand",
            "ArInvoiceCommandResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/ap-payments",
            "post",
            "CreateApPaymentCommand",
            "ApPaymentCommandResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/ap-payments/{paymentId}/allocations",
            "post",
            "AllocateApPaymentCommand",
            "ApPaymentCommandResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/ar-receipts",
            "post",
            "CreateArReceiptCommand",
            "ArReceiptCommandResultDto"
        );
        AssertRequestBodyAndSuccessResponse(
            document,
            "/api/ar-receipts/{receiptId}/allocations",
            "post",
            "AllocateArReceiptCommand",
            "ArReceiptCommandResultDto"
        );

        AssertResponseExists(document, "/api/users/whoami", "get", "200");
        AssertResponseExists(document, "/api/users/whoami", "get", "401");
        AssertResponseExists(document, "/api/users/admin-only", "get", "401");
        AssertResponseExists(document, "/api/users/admin-only", "get", "403");
        AssertResponseExists(document, "/api/users/user-only", "get", "401");
        AssertResponseExists(document, "/api/users/user-only", "get", "403");
    }

    [Test]
    public async Task SwaggerJson_Contains_Schema_Examples_For_Requests_And_Current_User_Response()
    {
        using var document = await GetSwaggerDocumentAsync();

        AssertSchemaExample(document, "RegisterDto");
        AssertSchemaExample(document, "LoginDto");
        AssertSchemaExample(document, "CreateApInvoiceCommand");
        AssertSchemaExample(document, "CreateArInvoiceCommand");
        AssertSchemaExample(document, "CreateApPaymentCommand");
        AssertSchemaExample(document, "AllocateApPaymentCommand");
        AssertSchemaExample(document, "CreateArReceiptCommand");
        AssertSchemaExample(document, "AllocateArReceiptCommand");
        AssertSchemaExample(document, "CurrentUserResponse");
        AssertArInvoiceCreateExampleShape(document);
    }

    private async Task<JsonDocument> GetSwaggerDocumentAsync()
    {
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    private static void AssertRoute(
        JsonDocument document,
        string path,
        string method,
        bool requiresBearer
    )
    {
        var route = GetPath(document, path);
        var operation = route.GetProperty(method);

        Assert.That(
            operation.TryGetProperty("summary", out _),
            Is.True,
            $"{path} summary missing."
        );
        Assert.That(
            operation.TryGetProperty("description", out _),
            Is.True,
            $"{path} description missing."
        );

        var hasSecurity = operation.TryGetProperty("security", out var security);
        Assert.That(hasSecurity, Is.EqualTo(requiresBearer), $"{path} security mismatch.");

        if (requiresBearer)
        {
            Assert.That(security.GetArrayLength(), Is.GreaterThan(0), $"{path} security empty.");
        }
    }

    private static void AssertRequestBodyAndSuccessResponse(
        JsonDocument document,
        string path,
        string method,
        string requestSchemaName,
        string responseSchemaName
    )
    {
        var operation = GetPath(document, path).GetProperty(method);
        var requestSchemaRef = operation
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.That(requestSchemaRef, Does.EndWith($"/{requestSchemaName}"));

        var responseSchemaRef = operation
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.That(responseSchemaRef, Does.EndWith($"/{responseSchemaName}"));
    }

    private static void AssertResponseExists(
        JsonDocument document,
        string path,
        string method,
        string statusCode
    )
    {
        var responses = GetPath(document, path).GetProperty(method).GetProperty("responses");

        Assert.That(
            responses.TryGetProperty(statusCode, out _),
            Is.True,
            $"{path} is missing response {statusCode}."
        );
    }

    private static void AssertSchemaExample(JsonDocument document, string schemaName)
    {
        var schema = document
            .RootElement.GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(schemaName);

        Assert.That(
            schema.TryGetProperty("example", out var example),
            Is.True,
            $"{schemaName} example missing."
        );
        Assert.That(example.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined));
    }

    private static void AssertArInvoiceCreateExampleShape(JsonDocument document)
    {
        var example = document
            .RootElement.GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("CreateArInvoiceCommand")
            .GetProperty("example");

        var lines = example.GetProperty("lines");
        Assert.That(
            lines.GetArrayLength(),
            Is.EqualTo(2),
            "CreateArInvoiceCommand lines example mismatch."
        );

        var serviceLine = lines[0];
        Assert.That(
            serviceLine.TryGetProperty("revenueAccount", out _),
            Is.True,
            "CreateArInvoiceCommand service-line example missing revenueAccount."
        );

        var itemLine = lines[1];
        Assert.That(
            itemLine.TryGetProperty("itemId", out _),
            Is.True,
            "CreateArInvoiceCommand item-line example missing itemId."
        );
        Assert.That(
            itemLine.TryGetProperty("locationId", out _),
            Is.True,
            "CreateArInvoiceCommand item-line example missing locationId."
        );
        Assert.That(
            itemLine.TryGetProperty("taxRateId", out _),
            Is.True,
            "CreateArInvoiceCommand item-line example missing taxRateId."
        );
    }

    private static JsonElement GetPath(JsonDocument document, string expectedPath)
    {
        var paths = document.RootElement.GetProperty("paths");
        var normalizedExpectedPath = NormalizePath(expectedPath);

        foreach (var property in paths.EnumerateObject())
        {
            if (NormalizePath(property.Name) == normalizedExpectedPath)
            {
                return property.Value;
            }
        }

        Assert.Fail($"Swagger path '{expectedPath}' was not found.");
        return default;
    }

    private static string NormalizePath(string path)
    {
        return Regex.Replace(path, @"\{([^}:]+):[^}]+\}", "{$1}").ToLowerInvariant();
    }
}
