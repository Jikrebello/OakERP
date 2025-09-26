using OakERP.Domain.Shared.Enums;

namespace OakERP.Domain.Entities.Posting;

public sealed class PostingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DocKind DocKind { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // optimistic concurrency safety
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");

    public ICollection<PostingRuleLine> Lines { get; set; } = new List<PostingRuleLine>();
}
