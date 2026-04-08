using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OakERP.API.Swagger.Configuration;

internal sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (HasAllowAnonymous(context))
        {
            return;
        }

        if (!HasAuthorize(context))
        {
            return;
        }

        if (operation.Security is { Count: > 0 })
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            },
        ];
    }

    private static bool HasAllowAnonymous(OperationFilterContext context)
    {
        return GetAttributes(context).OfType<IAllowAnonymous>().Any();
    }

    private static bool HasAuthorize(OperationFilterContext context)
    {
        return GetAttributes(context).OfType<IAuthorizeData>().Any();
    }

    private static IEnumerable<object> GetAttributes(OperationFilterContext context)
    {
        return context
            .MethodInfo.DeclaringType!.GetCustomAttributes(inherit: true)
            .Concat(context.MethodInfo.GetCustomAttributes(inherit: true));
    }
}
