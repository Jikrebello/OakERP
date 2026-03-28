using OakERP.Common.Enums;

namespace OakERP.Domain.Posting;

public sealed class PostingRuleLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostingRuleId { get; set; }

    public RuleSide Side { get; set; } // Debit / Credit
    public AccountKey AccountKey { get; set; } // logical role
    public AmountSource AmountSource { get; set; }

    /// <summary>
    /// Optional semantic scope (e.g. "Header", "Line.Stock", "Line.NonStock").
    /// This is just a discriminator the engine can interpret.
    /// </summary>
    public string Scope { get; set; } = "Header";

    public PostingRule PostingRule { get; set; } = default!;
}