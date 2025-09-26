namespace OakERP.Domain.Entities.Users;

/// <summary>
/// Represents a license associated with a tenant, including its unique identifier, key, creation date, and optional
/// expiration date.
/// </summary>
/// <remarks>A license is used to manage access or permissions for a specific tenant. It includes metadata such as
/// the creation date and an optional expiration date.</remarks>
public class License
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
