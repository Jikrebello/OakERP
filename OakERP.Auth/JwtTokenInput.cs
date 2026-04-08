namespace OakERP.Auth;

public sealed record JwtTokenInput(string UserId, string Email, Guid TenantId);
