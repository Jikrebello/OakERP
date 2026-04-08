using OakERP.Common.Enums;

namespace OakERP.Application.AccountsPayable;

public sealed class ApInvoiceSettlementSnapshotDto
{
    public Guid InvoiceId { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public DocStatus DocStatus { get; set; }
    public decimal DocTotal { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}
