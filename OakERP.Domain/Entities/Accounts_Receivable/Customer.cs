namespace OakERP.Domain.Entities.Accounts_Receivable;

public sealed class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public int TermsDays { get; set; } = 30;
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }

    public ICollection<ArInvoice> ArInvoices { get; set; } = [];
    public ICollection<ArReceipt> ArReceipts { get; set; } = [];
}