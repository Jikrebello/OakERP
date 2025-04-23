namespace OakERP.Domain.Entities;

public class License
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}