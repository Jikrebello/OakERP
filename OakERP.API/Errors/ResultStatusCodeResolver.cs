using Microsoft.AspNetCore.Http;
using OakERP.Common.Dtos.Base;
using OakERP.Common.Errors;

namespace OakERP.API.Errors;

internal static class ResultStatusCodeResolver
{
    public static int Resolve(BaseResultDto result)
    {
        return Resolve(result.FailureKind);
    }

    public static int Resolve(FailureKind? failureKind)
    {
        return (failureKind ?? FailureKind.Unexpected) switch
        {
            FailureKind.Validation => StatusCodes.Status400BadRequest,
            FailureKind.NotFound => StatusCodes.Status404NotFound,
            FailureKind.Conflict => StatusCodes.Status409Conflict,
            FailureKind.Unauthorized => StatusCodes.Status401Unauthorized,
            FailureKind.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
