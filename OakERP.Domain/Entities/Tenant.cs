namespace OakERP.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public License? License { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}