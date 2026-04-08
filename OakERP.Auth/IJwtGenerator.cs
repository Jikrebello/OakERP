namespace OakERP.Auth;

public interface IJwtGenerator
{
    string Generate(JwtTokenInput input);
}
