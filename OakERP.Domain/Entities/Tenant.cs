namespace OakERP.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
