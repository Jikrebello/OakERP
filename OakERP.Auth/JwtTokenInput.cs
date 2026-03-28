namespace OakERP.Auth;

/// <summary>
/// Minimal auth-local input contract for JWT generation.
/// </summary>
/// <remarks>This type is intentionally limited to the values currently required to produce the existing token claims.
/// It is not a second user model.</remarks>
public sealed record JwtTokenInput(string UserId, string Email, Guid TenantId);
