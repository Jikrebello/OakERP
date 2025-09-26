namespace OakERP.Domain.Entities.General_Ledger;

public sealed class FiscalPeriod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int FiscalYear { get; set; }
    public int PeriodNo { get; set; } // 1..12
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string Status { get; set; } = "open"; // open|closed
}
