namespace OakERP.Common.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
