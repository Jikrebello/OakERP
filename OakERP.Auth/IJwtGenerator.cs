using OakERP.Domain.Entities;

namespace OakERP.Auth;

public interface IJwtGenerator
{
    string Generate(ApplicationUser user);
}