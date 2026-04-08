namespace OakERP.Auth.Jwt;

public sealed record JwtTokenInput(string UserId, string Email, Guid TenantId);
