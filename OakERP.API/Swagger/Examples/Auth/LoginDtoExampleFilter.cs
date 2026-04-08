using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;
using OakERP.Common.Dtos.Auth;

namespace OakERP.API.Swagger.Examples.Auth;

internal sealed class LoginDtoExampleFilter : OpenApiExampleSchemaFilter<LoginDto>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["email"] = new OpenApiString("admin@acme.co.za"),
            ["password"] = new OpenApiString("AcmePass123!"),
        };
    }
}
