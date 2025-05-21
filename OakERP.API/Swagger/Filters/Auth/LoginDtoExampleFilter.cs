using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OakERP.Common.DTOs.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OakERP.API.Swagger.Filters.Auth;

/// <summary>
/// Provides an example schema for the <see cref="LoginDTO"/> type in OpenAPI documentation.
/// </summary>
/// <remarks>This filter is used to enhance the OpenAPI schema by adding an example object for the <see
/// cref="LoginDTO"/> type. The example includes typical values for the "email" and "password" fields, which can help
/// API consumers understand the expected structure and format of the request payload.</remarks>
public class LoginDtoExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(LoginDTO))
        {
            schema.Example = new OpenApiObject
            {
                ["email"] = new OpenApiString("user1@acme.com"),
                ["password"] = new OpenApiString("acmePass123"),
            };
        }
    }
}