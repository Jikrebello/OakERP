namespace OakERP.Domain.Entities.AccountsPayable;

public sealed class Vendor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string VendorCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public int TermsDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }
    public ICollection<ApInvoice> ApInvoices { get; set; } = [];
    public ICollection<ApPayment> ApPayments { get; set; } = [];
}
