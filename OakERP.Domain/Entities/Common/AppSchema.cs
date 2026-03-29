namespace OakERP.Domain.Entities.Common;

public sealed class AppSchema
{
    public string Name { get; set; } = default!;
    public string Sha256 { get; set; } = default!;
    public DateTime Updated_At { get; set; }
}
