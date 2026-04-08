namespace OakERP.Auth.Jwt;

public interface IJwtGenerator
{
    string Generate(JwtTokenInput input);
}
