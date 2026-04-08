using Microsoft.AspNetCore.Identity;
using OakERP.Domain.Entities.Users;

namespace OakERP.Auth;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public Tenant? Tenant { get; set; }
}
