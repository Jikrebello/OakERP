using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsPayable.Invoices.Commands;

public sealed class CreateApInvoiceCommand
{
    public string DocNo { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Memo { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DocTotal { get; set; }
    public IReadOnlyList<ApInvoiceLineInputDto> Lines { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
