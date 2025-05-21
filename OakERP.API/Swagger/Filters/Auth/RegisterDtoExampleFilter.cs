using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OakERP.Common.DTOs.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OakERP.API.Swagger.Filters.Auth;

/// <summary>
/// Provides an example schema for the <see cref="RegisterDTO"/> type in OpenAPI documentation.
/// </summary>
/// <remarks>This filter populates the example property of the OpenAPI schema for the <see cref="RegisterDTO"/>
/// type. The example includes sample values for the properties <c>tenantName</c>, <c>email</c>, <c>password</c>,  and
/// <c>confirmPassword</c>.</remarks>
public class RegisterDtoExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(RegisterDTO))
        {
            schema.Example = new OpenApiObject
            {
                ["tenantName"] = new OpenApiString("Acme Inc."),
                ["email"] = new OpenApiString("user1@acme.com"),
                ["password"] = new OpenApiString("acmePass123"),
                ["confirmPassword"] = new OpenApiString("acmePass123"),
            };
        }
    }
}