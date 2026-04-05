using OakERP.Common.Enums;

namespace OakERP.Application.AccountsPayable;

public sealed class ApInvoiceSnapshotDTO
{
    public Guid InvoiceId { get; set; }
    public string DocNo { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DocStatus DocStatus { get; set; }
    public string? Memo { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DocTotal { get; set; }
    public IReadOnlyList<ApInvoiceLineSnapshotDTO> Lines { get; set; } = [];
}
