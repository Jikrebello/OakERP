namespace OakERP.Domain.Entities;

/// <summary>
///Represents a tenant in a multi-tenant system, including its unique identifier, name, creation date, and associated
/// license information.
/// </summary>
/// <remarks>A tenant typically corresponds to a distinct customer or organizational unit in a multi-tenant
/// application.  The <see cref="Id"/> property uniquely identifies the tenant, while the <see cref="Name"/> property
/// provides a human-readable name. The <see cref="CreatedAt"/> property indicates when the tenant was created, and the
/// optional <see cref="License"/> property  contains information about the tenant's licensing, if applicable.</remarks>
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public License? License { get; set; }
}