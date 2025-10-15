using OakERP.Common.Enums;
using OakERP.Domain.Entities.Common;

namespace OakERP.Domain.Entities.Accounts_Receivable;

public sealed class ArInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocNo { get; set; } = default!;
    public Guid CustomerId { get; set; }

    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }

    public DateOnly? PostingDate { get; set; }

    public DocStatus Status { get; set; } = DocStatus.Draft;

    public string CurrencyCode { get; set; } = CurrencyISOCodes.ZAR.ToString();
    public Currency Currency { get; set; } = default!;

    public string? ShipTo { get; set; }
    public string? Memo { get; set; }

    public decimal TaxTotal { get; set; }
    public decimal DocTotal { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public Customer Customer { get; set; } = default!;

    public ICollection<ArInvoiceLine> Lines { get; set; } = [];
    public ICollection<ArReceiptAllocation> Allocations { get; set; } = [];
}
