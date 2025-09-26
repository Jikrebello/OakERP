using Microsoft.AspNetCore.Identity;

namespace OakERP.Domain.Entities.Users;

/// <summary>
/// Represents an application user with multi-tenancy support.
/// </summary>
/// <remarks>This class extends <see cref="IdentityUser"/> to include tenant-specific information,  enabling
/// multi-tenant functionality in the application.</remarks>
public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public Tenant? Tenant { get; set; }
}
