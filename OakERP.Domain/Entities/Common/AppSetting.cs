namespace OakERP.Domain.Entities.Common;

public sealed class AppSetting
{
    public string Key { get; set; } = default!;
    public string ValueJson { get; set; } = default!; // jsonb
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
