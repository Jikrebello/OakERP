namespace OakERP.Domain.Entities.Common;

public sealed class Currency
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Symbol { get; set; } = "R";
    public short Decimals { get; set; } = 2;
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Accounts_Payable.ApInvoice> ApInvoices { get; set; } = [];
}