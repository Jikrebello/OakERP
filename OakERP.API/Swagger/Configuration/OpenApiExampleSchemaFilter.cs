using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OakERP.API.Swagger.Configuration;

internal abstract class OpenApiExampleSchemaFilter<TSchema> : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(TSchema))
        {
            schema.Example = BuildExample();
        }
    }

    protected abstract IOpenApiAny BuildExample();
}
