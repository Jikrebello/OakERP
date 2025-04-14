using Microsoft.AspNetCore.Identity;

namespace OakERP.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
