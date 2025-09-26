using OakERP.Domain.Shared.Enums;

namespace OakERP.Domain.Entities.General_Ledger;

public sealed class GlAccount
{
    public string AccountNo { get; set; } = default!;
    public string Name { get; set; } = default!;
    public GlAccountType Type { get; set; }
    public string? ParentAccount { get; set; }
    public bool IsControl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GlAccount? Parent { get; set; }
    public ICollection<GlAccount> Children { get; set; } = new List<GlAccount>();
}
