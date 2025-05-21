namespace OakERP.Common.Abstractions;

public interface IPlatformService
{
    bool IsWeb { get; }
    bool IsHybrid { get; }
}