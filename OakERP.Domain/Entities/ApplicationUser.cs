using Microsoft.AspNetCore.Identity;

namespace OakERP.Domain.Entities;

/// <summary>
/// Represents an application user with multi-tenancy support.
/// </summary>
/// <remarks>This class extends <see cref="IdentityUser"/> to include tenant-specific information,  enabling
/// multi-tenant functionality in the application.</remarks>
public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}