using OakERP.Domain.Entities.Users;

namespace OakERP.Auth;

/// <summary>
/// Defines a contract for generating JSON Web Tokens (JWTs) for authenticated users.
/// </summary>
/// <remarks>Implementations of this interface are responsible for creating JWTs based on the provided user
/// information. The generated token can be used for authentication and authorization purposes in a secure
/// system.</remarks>
public interface IJwtGenerator
{
    string Generate(ApplicationUser user);
}
