using Microsoft.OpenApi.Any;
using OakERP.API.Controllers;
using OakERP.API.Swagger.Configuration;

namespace OakERP.API.Swagger.Examples.Users;

internal sealed class CurrentUserResponseExampleFilter
    : OpenApiExampleSchemaFilter<UsersController.CurrentUserResponse>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["userId"] = new OpenApiString("e9edb5e8-82dc-45c2-8ec1-266cc08d8919"),
            ["email"] = new OpenApiString("admin@acme.co.za"),
            ["tenantId"] = new OpenApiString("f0d61fe8-6394-49c9-a99b-3914ccfe2b72"),
        };
    }
}
