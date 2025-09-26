using OakERP.Domain.Shared.Enums;

namespace OakERP.Domain.Entities.Accounts_Payable;

public sealed class ApInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocNo { get; set; } = default!;
    public Guid VendorId { get; set; }
    public string InvoiceNo { get; set; } = default!;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DocStatus Status { get; set; } = DocStatus.Draft;
    public string Currency { get; set; } = "BASE";
    public string? Memo { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DocTotal { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public Vendor Vendor { get; set; } = default!;
    public ICollection<ApInvoiceLine> Lines { get; set; } = new List<ApInvoiceLine>();
}
