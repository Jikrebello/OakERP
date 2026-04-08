using Microsoft.AspNetCore.Identity;
using OakERP.Common.Dtos.Auth;

namespace OakERP.Auth.Services;

internal sealed class IdentityFailureMapper
{
    public AuthResultDto MapCreateFailure(IdentityResult createUserResult)
    {
        string message =
            createUserResult.Errors.FirstOrDefault()?.Description
            ?? AuthErrors.UnexpectedRegistrationFailure.Message;

        return AuthResultDto.Fail(AuthErrors.IdentityCreateFailed(message));
    }
}
