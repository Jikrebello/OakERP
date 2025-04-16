using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OakERP.Shared.DTOs.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OakERP.WebAPI.Swagger.Filters.Auth;

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