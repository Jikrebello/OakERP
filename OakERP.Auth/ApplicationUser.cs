using Microsoft.AspNetCore.Identity;

namespace OakERP.Domain.Entities.Users;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public Tenant? Tenant { get; set; }
}
