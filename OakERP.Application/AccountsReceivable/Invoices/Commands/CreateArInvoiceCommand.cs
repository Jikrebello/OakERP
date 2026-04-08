using System.Text.Json.Serialization;

namespace OakERP.Application.AccountsReceivable.Invoices.Commands;

public sealed class CreateArInvoiceCommand
{
    public string DocNo { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? ShipTo { get; set; }
    public string? Memo { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DocTotal { get; set; }
    public IReadOnlyList<ArInvoiceLineInputDto> Lines { get; set; } = [];

    [JsonIgnore]
    public string? PerformedBy { get; set; }
}
