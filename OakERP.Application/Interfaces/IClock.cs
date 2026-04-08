namespace OakERP.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
