using OakERP.Application.Interfaces;

namespace OakERP.Application.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
