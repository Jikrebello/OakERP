using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;
using OakERP.Common.Dtos.Auth;

namespace OakERP.API.Swagger.Examples.Auth;

internal sealed class RegisterDtoExampleFilter : OpenApiExampleSchemaFilter<RegisterDto>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["tenantName"] = new OpenApiString("Acme Manufacturing"),
            ["firstName"] = new OpenApiString("James"),
            ["lastName"] = new OpenApiString("Meyer"),
            ["phoneNumber"] = new OpenApiString("+27110001111"),
            ["email"] = new OpenApiString("admin@acme.co.za"),
            ["password"] = new OpenApiString("AcmePass123!"),
            ["confirmPassword"] = new OpenApiString("AcmePass123!"),
        };
    }
}
