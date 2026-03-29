using OakERP.Common.Enums;

namespace OakERP.Domain.Posting;

public sealed class PostingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DocKind DocKind { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public ICollection<PostingRuleLine> Lines { get; set; } = [];
}
