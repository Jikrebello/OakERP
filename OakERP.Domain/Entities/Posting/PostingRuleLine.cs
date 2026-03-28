using OakERP.Common.Enums;

namespace OakERP.Domain.Entities.Posting;

public sealed class PostingRuleLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostingRuleId { get; set; }

    public RuleSide Side { get; set; }
    public AccountKey AccountKey { get; set; }
    public RuleScope Scope { get; set; }
    public AmountSource AmountKey { get; set; }

    // Optional simple conditioning (keeps logic data-driven without a DSL)
    public bool OnlyWhenHasTax { get; set; } // e.g., tax lines on AR/AP invoices

    public bool OnlyForStockLines { get; set; } // e.g., COGS/Inventory for stock items

    // nav (domain-only; EF will map in Infrastructure)
    public PostingRule? PostingRule { get; set; }
}