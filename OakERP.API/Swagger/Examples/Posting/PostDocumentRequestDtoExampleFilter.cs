using Microsoft.OpenApi.Any;
using OakERP.API.Contracts.Posting;
using OakERP.API.Swagger.Configuration;

namespace OakERP.API.Swagger.Examples.Posting;

internal sealed class PostDocumentRequestDtoExampleFilter
    : OpenApiExampleSchemaFilter<PostDocumentRequestDto>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["postingDate"] = new OpenApiString("2026-04-09"),
            ["force"] = new OpenApiBoolean(false),
        };
    }
}
