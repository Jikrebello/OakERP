using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OakERP.Common.Dtos.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OakERP.API.Swagger.Filters.Auth;

/// <summary>
/// Provides an example schema for the <see cref="RegisterDto"/> type in OpenAPI documentation.
/// </summary>
/// <remarks>This filter populates the OpenAPI schema with a sample object for the <see cref="RegisterDto"/> type,
/// demonstrating typical values for its properties. The example includes fields such as tenant name, first name, last
/// name, phone number, email, password, and confirm password.</remarks>
public class RegisterDtoExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(RegisterDto))
        {
            schema.Example = new OpenApiObject
            {
                ["tenantName"] = new OpenApiString("Acme Inc."),
                ["firstName"] = new OpenApiString("John"),
                ["lastName"] = new OpenApiString("Doe"),
                ["phoneNumber"] = new OpenApiString("+1234567890"),
                ["email"] = new OpenApiString("user1@acme.com"),
                ["password"] = new OpenApiString("acmePass123"),
                ["confirmPassword"] = new OpenApiString("acmePass123"),
            };
        }
    }
}
